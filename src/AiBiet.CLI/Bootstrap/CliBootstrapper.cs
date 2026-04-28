using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace AiBiet.CLI.Bootstrap;

internal static class CliBootstrapper
{
    public static CommandApp Build(ServiceCollection services)
    {
        // Scan for tool registrations first
        using var serviceProvider = services.BuildServiceProvider();
        var toolManager = serviceProvider.GetRequiredService<IToolManager>();

        var toolRegistrations = toolManager.GetToolRegistrationsAsync().GetAwaiter().GetResult();

        // Register tool command types with DI container before creating the registrar
        var commandTypes = new List<(Type commandType, ToolRegistrationInfo registration)>();
        foreach (var registration in toolRegistrations)
        {
            var commandType = typeof(ToolCommandWrapper<,>).MakeGenericType(registration.ToolType, registration.SettingsType);
            services.AddTransient(commandType);
            commandTypes.Add((commandType, registration));
        }

        var registrar = new TypeRegistrar(services);
        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("aibiet");
            config.SetApplicationVersion(AppInfo.GetVersion());

            CommandRegistration.Register(config);

            // Dynamically register installed tools as top-level commands
            foreach (var (commandType, registration) in commandTypes)
            {
                try
                {
                    var commandName = registration.Name.ToLowerInvariant();
                    
                    var addCommandMethod = typeof(IConfigurator)
                        .GetMethods()
                        .FirstOrDefault(m => m is { Name: "AddCommand", IsGenericMethod: true } &&
                                             m.GetParameters().Length == 1 &&
                                             m.GetParameters()[0].ParameterType == typeof(string));

                    if (addCommandMethod != null)
                    {
                        var genericMethod = addCommandMethod.MakeGenericMethod(commandType);
                        var commandConfigurator = genericMethod.Invoke(config, [commandName]);

                        if (commandConfigurator != null)
                        {
                            var withDescriptionMethod = commandConfigurator.GetType().GetMethod("WithDescription");
                            withDescriptionMethod?.Invoke(commandConfigurator, [registration.Description]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Warning: Failed to register tool '{registration.Name}': {ex.Message}");
                }
            }
        });

        return app;
    }
}