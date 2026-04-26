using System.ComponentModel;

using AiBiet.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolUpdateSettings : CommandSettings
{
    [CommandArgument(0, "<TOOL_NAME>")]
    [Description("The name of the tool to update")]
    public string ToolName { get; set; } = "";
}

internal class ToolUpdateCommand(IToolManager toolManager) : AsyncCommand<ToolUpdateSettings>
{
    private readonly IToolManager _toolManager = toolManager;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolUpdateSettings settings, CancellationToken cancellationToken)
    {
        var toolName = settings.ToolName;

        await AnsiConsole.Status()
            .StartAsync($"Updating tool '{toolName}'...", async ctx =>
            {
                var success = await _toolManager.UpdateToolAsync(toolName, cancellationToken).ConfigureAwait(false);
                if (success)
                {
                    AnsiConsole.MarkupLine($"[green]Successfully updated tool '{toolName}'.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]No update found or tool '{toolName}' is not installed.[/]");
                }
            }).ConfigureAwait(false);

        return 0;
    }
}
