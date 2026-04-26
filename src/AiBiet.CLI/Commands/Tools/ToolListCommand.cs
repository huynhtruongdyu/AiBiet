using System.ComponentModel;

using AiBiet.Core.Domain.Models;
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

internal class ToolListCommand(IToolManager toolManager) : AsyncCommand<ToolListSettings>
{
    private readonly IToolManager _toolManager = toolManager;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolListSettings settings, CancellationToken cancellationToken)
    {
        List<(string Name, string? Description, string Source)> tools;
        if (settings.Online)
        {
            var allTools = await _toolManager.ListAvailableToolsAsync(cancellationToken).ConfigureAwait(false);
            tools = allTools.Select(t => (t.Name, t.Description, t.Source)).ToList();
        }
        else
        {
            var registrations = await _toolManager.GetToolRegistrationsAsync(cancellationToken).ConfigureAwait(false);
            tools = registrations.Select(r => (r.Name, (string?)r.Description, "Local")).ToList();
        }

        if (tools.Count == 0)
        {
            AnsiConsole.MarkupLine(settings.Online 
                ? "[yellow]No tools found in configured sources.[/]" 
                : "[yellow]No tools installed. Use [blue]--online[/] to search for available tools.[/]");
            return 0;
        }

        var table = new Table();
        table.Title(settings.Online ? "[blue]Available Tools[/]" : "[green]Installed Tools[/]");
        table.AddColumn("Name");
        table.AddColumn("Description");
        table.AddColumn("Source");

        foreach (var tool in tools)
        {
            table.AddRow(tool.Name, tool.Description ?? "", tool.Source);
        }

        AnsiConsole.Write(table);

        return 0;
    }
}