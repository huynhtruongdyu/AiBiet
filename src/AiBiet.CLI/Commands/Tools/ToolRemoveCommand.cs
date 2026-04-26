using System.ComponentModel;

using AiBiet.CLI.Infrastructure;
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

internal class ToolRemoveCommand(AiBietConfig config, IToolScanner toolScanner) : AsyncCommand<ToolRemoveSettings>
{
    private readonly AiBietConfig _config = config;
    private readonly IToolScanner _toolScanner = toolScanner;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolRemoveSettings settings, CancellationToken cancellationToken)
    {
        var toolName = settings.ToolName;

        await AnsiConsole.Status()
            .StartAsync($"Removing tool '{toolName}'...", async ctx =>
            {
                var tool = await _toolScanner.FindToolAsync(toolName, [_config.ToolsPath], cancellationToken).ConfigureAwait(false);

                if (tool == null)
                {
                    AnsiConsole.MarkupLine($"[red]Tool '{toolName}' is not installed.[/]");
                    return;
                }

                try
                {
                    if (File.Exists(tool.PackagePath))
                    {
                        File.Delete(tool.PackagePath);
                        AnsiConsole.MarkupLine($"[green]Successfully removed tool '{toolName}'.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]Package file not found: {tool.PackagePath}[/]");
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to remove tool: {ex.Message}[/]");
                }
            }).ConfigureAwait(false);

        return 0;
    }
}
