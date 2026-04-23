using AiBiet.CLI.Commands;
using AiBiet.CLI.Commands.Utils;
using AiBiet.CLI.Infrastructure;
using AiBiet.CLI.UI;
using AiBiet.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

var appConfig = await ConfigBootstrapper.InitializeAsync().ConfigureAwait(false);

var services = new ServiceCollection();
services.AddSingleton(appConfig);
services.AddHttpClient();

// Register AI provider resolver (reads config at runtime)
services.AddSingleton<AiProviderResolver>();
services.AddTransient<IAiProvider>(sp =>
{
    var config = sp.GetRequiredService<AiBietConfig>();
    var resolver = sp.GetRequiredService<AiProviderResolver>();
    return resolver.Resolve(config);
});

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.SetApplicationName("aibiet");
    config.SetApplicationVersion(AppInfo.GetVersion());

    config.AddCommand<AskCommand>("ask")
       .WithDescription("Ask a model a single question");

    config.AddCommand<ChatCommand>("chat")
        .WithDescription("Start an interactive chat session");

    config.AddCommand<ModelsCommand>("models")
        .WithDescription("(Mocked) List available models");

    config.AddCommand<ConfigCommand>("config")
        .WithDescription("Show current configurations");

    config.AddCommand<DoctorCommand>("doctor")
        .WithDescription("Check system health and connectivity");

    config.AddBranch("utils", utils =>
    {
        utils.SetDescription("Everyday developer utilities");

        utils.AddCommand<GuidCommand>("guid")
            .WithDescription("Generate one or more GUIDs/UUIDs");
    });
});

if (args.Length == 1 && (args[0] == "-v" || args[0] == "-V"))
{
    args = ["--version"];
}

if (args.Length == 0)
{
    SplashScreen.Show();
    args = ["-h"];
}
return await app.RunAsync(args).ConfigureAwait(false);