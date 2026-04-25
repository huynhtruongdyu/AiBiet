using AiBiet.CLI.Commands;
using AiBiet.CLI.Commands.Utils;

using Spectre.Console.Cli;

namespace AiBiet.CLI.Bootstrap;

internal static class CommandRegistration
{
    public static void Register(IConfigurator config)
    {
        config.AddCommand<AskCommand>("ask")
            .WithDescription("Ask a model a single question");

        config.AddCommand<ChatCommand>("chat")
            .WithDescription("Start an interactive chat session");

        config.AddCommand<ModelsCommand>("models")
            .WithDescription("List available models");

        config.AddCommand<ConfigCommand>("config")
            .WithDescription("Show current configurations");

        config.AddCommand<DoctorCommand>("doctor")
            .WithDescription("Check system health");

        config.AddBranch("utils", utils =>
        {
            utils.SetDescription("Everyday developer utilities");

            utils.AddCommand<GuidCommand>("guid")
                .WithDescription("Generate GUIDs");
        });
    }
}