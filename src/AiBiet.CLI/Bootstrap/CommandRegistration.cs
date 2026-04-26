using AiBiet.CLI.Commands;
using AiBiet.CLI.Commands.Tools;
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

        // config.AddCommand<ModelsCommand>("models")
        //     .WithDescription("List available models");

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

        config.AddBranch("tool", tools =>
        {
            tools.SetDescription("Manage tools");

            tools.AddBranch("source", source =>
            {
                source.SetDescription("Manage tool sources");

                // source.AddCommand<ToolSourceAddCommand>("add")
                //     .WithDescription("Add tool sources");

                // source.AddCommand<ToolSourceRemoveCommand>("remove")
                //     .WithDescription("Remove tool sources");

                source.AddCommand<ToolSourceListCommand>("list")
                    .WithDescription("List tool sources");
            });

            // tools.AddCommand<ToolInstallCommand>("install")
            //     .WithDescription("Install a tool");

            tools.AddCommand<ToolAddCommand>("add")
                .WithDescription("Add/Install a tool");

            tools.AddCommand<ToolUpdateCommand>("update")
                .WithDescription("Update an installed tool");

            tools.AddCommand<ToolRemoveCommand>("remove")
                .WithDescription("Remove an installed tool");

            tools.AddCommand<ToolListCommand>("list")
                .WithDescription("List tools (installed or available)");

        });
    }
}