using AiBiet.CLI.Commands;
using AiBiet.CLI.Commands.Utils;
using AiBiet.CLI.Infrastructure;
using AiBiet.CLI.UI;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

var services = new ServiceCollection();
var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.SetApplicationName("aibiet");

    config.AddCommand<AskCommand>("ask")
       .WithDescription("(Mocked) Ask a model a single question");

    config.AddCommand<ChatCommand>("chat")
        .WithDescription("(Mocked) Start interactive chat mode");

    config.AddCommand<ModelsCommand>("models")
        .WithDescription("(Mocked) List available models");

    config.AddBranch("utils", utils =>
    {
        utils.SetDescription("Everyday developer utilities");

        utils.AddCommand<GuidCommand>("guid")
            .WithDescription("Generate one or more GUIDs/UUIDs");
    });
});

if (args.Length == 0)
{
    SplashScreen.Show();
    args = ["-h"];
}
return await app.RunAsync(args).ConfigureAwait(false);