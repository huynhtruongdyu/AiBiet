using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands;

internal class ModelsCommand : Command
{

    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var table = new Table();

        table.AddColumn("Provider");
        table.AddColumn("Model");

        table.AddRow("Ollama", "llama3");
        table.AddRow("OpenAI", "gpt-4o");
        table.AddRow("Anthropic", "claude");

        AnsiConsole.Write(table);

        return 0;
    }
}