using AiBiet.CLI.Commands;
using AiBiet.CLI.Commands.Tools;
using AiBiet.CLI.Commands.Utils;
using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;
using AiBiet.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace AiBiet.CLI.Bootstrap;

internal static class ServiceRegistration
{
    public static ServiceCollection Configure(AiBietConfig appConfig)
    {
        var services = new ServiceCollection();

        services.AddSingleton(appConfig);
        services.AddHttpClient();

        // Infrastructure Services
        services.AddSingleton<IAiProviderFactory, AiProviderFactory>();
        services.AddSingleton<IToolManager, ToolManager>();
        services.AddSingleton<AiProviderResolver>();

        // Default AI Provider
        services.AddTransient<IAiProvider>(sp =>
        {
            var factory = sp.GetRequiredService<IAiProviderFactory>();
            return factory.GetDefaultProvider();
        });

        // Register commands as services (required by Spectre.Console.Cli)
        services.AddSingleton<AskCommand>();
        services.AddSingleton<ChatCommand>();
        services.AddSingleton<ModelsCommand>();
        services.AddSingleton<ConfigCommand>();
        services.AddSingleton<DoctorCommand>();
        services.AddSingleton<GuidCommand>();
        services.AddSingleton<ToolSourceListCommand>();
        services.AddSingleton<ToolListCommand>();
        services.AddSingleton<ToolAddCommand>();
        services.AddSingleton<ToolUpdateCommand>();
        services.AddSingleton<ToolRemoveCommand>();

        return services;
    }
}
