using Spectre.Console;
using Spectre.Console.Cli;
using AiBiet.CLI.Infrastructure;

namespace AiBiet.CLI.Commands;

internal class ConfigCommand : Command
{
    private readonly AiBietConfig _config;

    public ConfigCommand(AiBietConfig config)
    {
        _config = config;
    }

    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aibiet", "config.json");

        AnsiConsole.MarkupLine($"[bold]Configuration File:[/] {configPath}");
        AnsiConsole.WriteLine();

        var table = new Table();

        table.AddColumn("Key");
        table.AddColumn("Value");

        table.AddRow("DefaultProvider", _config.DefaultProvider);

        AnsiConsole.Write(table);

        return 0;
    }
}
