using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace AiBiet.CLI.Bootstrap;

internal static class CliBootstrapper
{
    public static CommandApp Build(ServiceCollection services)
    {
        var registrar = new TypeRegistrar(services);

        var app = new CommandApp(registrar);

        // Build a temporary service provider to scan and register tools
        using var serviceProvider = services.BuildServiceProvider();
        var appConfig = serviceProvider.GetRequiredService<AiBietConfig>();
        var toolScanner = serviceProvider.GetRequiredService<IToolScanner>();

        var toolRegistrations = toolScanner.GetToolRegistrationsAsync(appConfig.ToolSources).GetAwaiter().GetResult();

        app.Configure(config =>
        {
            config.SetApplicationName("aibiet");
            config.SetApplicationVersion(AppInfo.GetVersion());

            CommandRegistration.Register(config);

            // Dynamically register installed tools as top-level commands
            foreach (var registration in toolRegistrations)
            {
                var commandType = typeof(ToolCommandWrapper<,>).MakeGenericType(registration.ToolType, registration.SettingsType);

                // Use reflection to call the generic AddCommand<T>(name) method
                var addCommandMethod = typeof(IConfigurator)
                    .GetMethods()
                    .FirstOrDefault(m => m is { Name: "AddCommand", IsGenericMethod: true } &&
                                         m.GetParameters().Length == 1 &&
                                         m.GetParameters()[0].ParameterType == typeof(string));

                if (addCommandMethod != null)
                {
                    var genericMethod = addCommandMethod.MakeGenericMethod(commandType);
                    var commandConfigurator = genericMethod.Invoke(config, [registration.Name]);

                    if (commandConfigurator != null)
                    {
                        var withDescriptionMethod = commandConfigurator.GetType().GetMethod("WithDescription");
                        withDescriptionMethod?.Invoke(commandConfigurator, [registration.Description]);
                    }
                }
            }
        });

        return app;
    }
}