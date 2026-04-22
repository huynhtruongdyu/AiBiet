using AiBiet.CLI.Commands;
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
       .WithDescription("Ask a model a single question");

    config.AddCommand<ChatCommand>("chat")
        .WithDescription("Start interactive chat mode");

    config.AddCommand<ModelsCommand>("models")
        .WithDescription("List available models");
});


// Show splash only when no command is passed
if (args.Length == 0)
{
    SplashScreen.Show();
    return 0;
}
return await app.RunAsync(args).ConfigureAwait(false);