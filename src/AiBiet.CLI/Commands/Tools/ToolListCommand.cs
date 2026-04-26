using System.ComponentModel;

using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolListSettings : CommandSettings
{
    [CommandOption("-o|--online")]
    [Description("Scan all configured sources for available tools")]
    public bool Online { get; set; }
}

internal class ToolListCommand : AsyncCommand<ToolListSettings>
{
    private readonly AiBietConfig _config;
    private readonly IToolScanner _toolScanner;

    public ToolListCommand(AiBietConfig config, IToolScanner toolScanner)
    {
        _config = config;
        _toolScanner = toolScanner;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolListSettings settings, CancellationToken cancellationToken)
    {
        var installedTools = (await _toolScanner.ScanAsync([_config.ToolsPath], cancellationToken).ConfigureAwait(false))
            .Select(t => t.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sources = settings.Online
            ? _config.ToolSources
            : [_config.ToolsPath];

        if (sources.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No tool sources configured.[/]");
            return 0;
        }

        var tools = (await _toolScanner.ScanAsync(sources, cancellationToken).ConfigureAwait(false)).ToList();

        if (tools.Count == 0)
        {
            if (settings.Online)
            {
                AnsiConsole.MarkupLine("[yellow]No tools found in configured sources.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No tools installed. Use [blue]--online[/] to search for available tools.[/]");
            }
            return 0;
        }

        var table = new Table();
        table.Title(settings.Online ? "[blue]Available Tools[/]" : "[green]Installed Tools[/]");
        table.AddColumn("Name");
        table.AddColumn("Description");
        table.AddColumn("Source");
        table.AddColumn("Status");

        foreach (var tool in tools)
        {
            var isInstalled = installedTools.Contains(tool.Name);
            var status = isInstalled ? "[green]Installed[/]" : "[blue]Available[/]";
            table.AddRow(tool.Name, tool.Description ?? "", tool.Source, status);
        }

        AnsiConsole.Write(table);

        return 0;
    }
}