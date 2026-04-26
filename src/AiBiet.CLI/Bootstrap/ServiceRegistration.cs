using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace AiBiet.CLI.Bootstrap;

internal static class ServiceRegistration
{
    public static ServiceCollection Configure(AiBietConfig appConfig)
    {
        var services = new ServiceCollection();

        services.AddSingleton(appConfig);
        services.AddHttpClient();

        services.AddSingleton<AiProviderResolver>();
        services.AddSingleton<IToolScanner, ToolScanner>();

        services.AddTransient<IAiProvider>(sp =>
        {
            var config = sp.GetRequiredService<AiBietConfig>();
            var resolver = sp.GetRequiredService<AiProviderResolver>();
            return resolver.Resolve(config);
        });

        return services;
    }
}
