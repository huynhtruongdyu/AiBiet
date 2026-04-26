using System.ComponentModel;

using AiBiet.CLI.Infrastructure;
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

internal class ToolAddCommand(AiBietConfig config, IToolScanner toolScanner) : AsyncCommand<ToolAddSettings>
{
    private readonly AiBietConfig _config = config;
    private readonly IToolScanner _toolScanner = toolScanner;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolAddSettings settings, CancellationToken cancellationToken)
    {
        var toolName = settings.ToolName;

        await AnsiConsole.Status()
            .StartAsync($"Searching for tool '{toolName}'...", async ctx =>
            {
                // Check if already installed
                var installedTools = await _toolScanner.ScanAsync([_config.ToolsPath], cancellationToken).ConfigureAwait(false);
                if (installedTools.Any(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase)))
                {
                    AnsiConsole.MarkupLine($"[yellow]Tool '{toolName}' is already installed.[/]");
                    return;
                }

                // Search in other sources
                var otherSources = _config.ToolSources.Where(s => !string.Equals(s, _config.ToolsPath, StringComparison.OrdinalIgnoreCase));
                var tool = await _toolScanner.FindToolAsync(toolName, otherSources, cancellationToken).ConfigureAwait(false);

                if (tool == null)
                {
                    AnsiConsole.MarkupLine($"[red]Tool '{toolName}' not found in configured sources.[/]");
                    return;
                }

                AnsiConsole.MarkupLine($"[blue]Installing tool '{toolName}' from {tool.Source}...[/]");

                try
                {
                    var fileName = Path.GetFileName(tool.PackagePath);
                    var destination = Path.Combine(_config.ToolsPath, fileName);
                    File.Copy(tool.PackagePath, destination, true);
                    AnsiConsole.MarkupLine($"[green]Successfully installed tool '{toolName}'.[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to install tool: {ex.Message}[/]");
                }
            }).ConfigureAwait(false);

        return 0;
    }
}
