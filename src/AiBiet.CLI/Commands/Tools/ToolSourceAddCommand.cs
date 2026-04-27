using System.ComponentModel;

using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Domain.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolSourceAddSettings : CommandSettings
{
    [CommandArgument(0, "<SOURCE>")]
    [Description("The source path or URL to add")]
    public string Source { get; set; } = "";
}

internal class ToolSourceAddCommand(AiBietConfig config) : AsyncCommand<ToolSourceAddSettings>
{
    private readonly AiBietConfig _config = config;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolSourceAddSettings settings, CancellationToken cancellationToken)
    {
        var source = settings.Source;

        if (_config.ToolSources.Contains(source, StringComparer.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"[yellow]Source '{source}' is already configured.[/]");
            return 0;
        }

        _config.ToolSources.Add(source);
        await ConfigBootstrapper.SaveAsync(_config).ConfigureAwait(false);

        AnsiConsole.MarkupLine($"[green]Successfully added source '{source}'.[/]");

        return 0;
    }
}
