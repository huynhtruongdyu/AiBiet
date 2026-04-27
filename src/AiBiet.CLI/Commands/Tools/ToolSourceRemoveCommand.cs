using System.ComponentModel;

using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Domain.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolSourceRemoveSettings : CommandSettings
{
    [CommandArgument(0, "<SOURCE>")]
    [Description("The source path or URL to remove")]
    public string Source { get; set; } = "";
}

internal class ToolSourceRemoveCommand(AiBietConfig config) : AsyncCommand<ToolSourceRemoveSettings>
{
    private readonly AiBietConfig _config = config;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolSourceRemoveSettings settings, CancellationToken cancellationToken)
    {
        var source = settings.Source;

        // Try to find the source case-insensitively
        var existingSource = _config.ToolSources.FirstOrDefault(s => string.Equals(s, source, StringComparison.OrdinalIgnoreCase));
        
        if (existingSource != null)
        {
            _config.ToolSources.Remove(existingSource);
            await ConfigBootstrapper.SaveAsync(_config).ConfigureAwait(false);
            AnsiConsole.MarkupLine($"[green]Successfully removed source '{existingSource}'.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]Source '{source}' not found.[/]");
        }

        return 0;
    }
}
