using AiBiet.CLI.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Tools;

internal class ToolSourceListSettings : CommandSettings
{
}

internal class ToolSourceListCommand(AiBietConfig config) : AsyncCommand<ToolSourceListSettings>
{
    private readonly AiBietConfig _config = config;

    protected override async Task<int> ExecuteAsync(CommandContext context, ToolSourceListSettings settings, CancellationToken cancellationToken)
    {
        var sources = _config.ToolSources;

        if (sources.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No tool sources configured.[/]");
            AnsiConsole.MarkupLine("Add sources using: [green]aibiet tools source add <path>[/]");
            return 0;
        }

        var table = new Table();
        table.AddColumn("#");
        table.AddColumn("Source");

        for (int i = 0; i < sources.Count; i++)
        {
            table.AddRow((i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), sources[i]);
        }

        AnsiConsole.Write(table);

        return await Task.FromResult(0).ConfigureAwait(false);
    }
}
