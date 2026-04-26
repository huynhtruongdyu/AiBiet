using System.ComponentModel;

using AiBiet.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolRemoveSettings : CommandSettings
{
    [CommandArgument(0, "<TOOL_NAME>")]
    [Description("The name of the tool to remove")]
    public string ToolName { get; set; } = "";
}

internal class ToolRemoveCommand(IToolManager toolManager) : AsyncCommand<ToolRemoveSettings>
{
    private readonly IToolManager _toolManager = toolManager;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolRemoveSettings settings, CancellationToken cancellationToken)
    {
        var toolName = settings.ToolName;

        await AnsiConsole.Status()
            .StartAsync($"Removing tool '{toolName}'...", async ctx =>
            {
                var success = await _toolManager.RemoveToolAsync(toolName, cancellationToken).ConfigureAwait(false);
                if (success)
                {
                    AnsiConsole.MarkupLine($"[green]Successfully removed tool '{toolName}'.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Tool '{toolName}' is not installed.[/]");
                }
            }).ConfigureAwait(false);

        return 0;
    }
}
