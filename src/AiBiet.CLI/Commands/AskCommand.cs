using System.ComponentModel;

using Spectre.Console;
using Spectre.Console.Cli;

using AiBiet.CLI.Infrastructure;

namespace AiBiet.CLI.Commands;

internal class AskCommandSettings : CommandSettings
{
    [CommandOption("-m|--model")]
    [Description("Model name. Uses default from config if omitted.")]
    public string? Model { get; set; }

    [CommandOption("-p|--prompt")]
    [Description("Prompt text")]
    public string Prompt { get; set; } = string.Empty;
}


internal class AskCommand : Command<AskCommandSettings>
{
    private readonly AiBietConfig _config;

    public AskCommand(AiBietConfig config)
    {
        _config = config;
    }

    protected override int Execute(CommandContext context, AskCommandSettings settings, CancellationToken cancellationToken)
    {
        var model = settings.Model ?? _config.DefaultProvider;

        AnsiConsole.MarkupLine($"[green]Model:[/] {model}");
        AnsiConsole.MarkupLine($"[yellow]Prompt:[/] {settings.Prompt}");

        AnsiConsole.MarkupLine("[blue]Response:[/] Hello from AI");
        return 0;
    }
}