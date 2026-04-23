using System.ComponentModel;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands;

internal class AskCommandSettings : CommandSettings
{
    [CommandOption("-m|--model")]
    [Description("Model name")]
    public string Model { get; set; } = "ollama";

    [CommandOption("-p|--prompt")]
    [Description("Prompt text")]
    public string Prompt { get; set; } = string.Empty;
}


internal class AskCommand : Command<AskCommandSettings>
{
    protected override int Execute(CommandContext context, AskCommandSettings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine($"[green]Model:[/] {settings.Model}");
        AnsiConsole.MarkupLine($"[yellow]Prompt:[/] {settings.Prompt}");

        AnsiConsole.MarkupLine("[blue]Response:[/] Hello from AI");
        return 0;
    }
}