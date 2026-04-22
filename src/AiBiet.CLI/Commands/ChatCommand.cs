using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands;

internal class ChatCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("[green]Interactive chat mode[/]");
        AnsiConsole.MarkupLine("Type [yellow]exit[/] to quit");

        while (true)
        {
            var input = AnsiConsole.Ask<string>("[blue]>[/]");

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            AnsiConsole.MarkupLine($"[green]AI:[/] You said: {input}");
        }

        return 0;
    }
}