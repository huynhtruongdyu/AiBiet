using System.ComponentModel;

using AiBiet.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolAddSettings : CommandSettings
{
    [CommandArgument(0, "<TOOL_NAME>")]
    [Description("The name of the tool to add")]
    public string ToolName { get; set; } = "";
}

internal class ToolAddCommand(IToolManager toolManager) : AsyncCommand<ToolAddSettings>
{
    private readonly IToolManager _toolManager = toolManager;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolAddSettings settings, CancellationToken cancellationToken)
    {
        var toolName = settings.ToolName;

        await AnsiConsole.Status()
            .StartAsync($"Installing tool '{toolName}'...", async ctx =>
            {
                var success = await _toolManager.InstallToolAsync(toolName, cancellationToken).ConfigureAwait(false);
                if (success)
                {
                    AnsiConsole.MarkupLine($"[green]Successfully installed tool '{toolName}'.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Tool '{toolName}' not found or already installed.[/]");
                }
            }).ConfigureAwait(false);

        return 0;
    }
}
