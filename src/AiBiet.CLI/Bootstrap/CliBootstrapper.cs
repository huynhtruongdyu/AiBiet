using AiBiet.CLI.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace AiBiet.CLI.Bootstrap;

internal static class CliBootstrapper
{
    public static CommandApp Build(ServiceCollection services)
    {
        var registrar = new TypeRegistrar(services);

        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("aibiet");
            config.SetApplicationVersion(AppInfo.GetVersion());

            CommandRegistration.Register(config);
        });

        return app;
    }
}