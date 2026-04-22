using System.Reflection;

using Spectre.Console;

namespace AiBiet.CLI.UI;

internal static class SplashScreen
{
    public static void Show()
    {
        var version = GetVersion();

        AnsiConsole.Write(
            new FigletText("AI BIET")
                .LeftJustified());

        AnsiConsole.MarkupLine($"[grey]v{version}[/]");
        AnsiConsole.MarkupLine("[grey]Universal AI runtime for every model[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[yellow]Available commands:[/]");
        AnsiConsole.MarkupLine("  [green]ask[/]     Ask a model");
        AnsiConsole.MarkupLine("  [green]chat[/]    Start interactive chat mode");
        AnsiConsole.MarkupLine("  [green]models[/]  List available models");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[grey]Example:[/] aibiet ask -m ollama -p \"hello\"");
        AnsiConsole.WriteLine();
    }

    private static string GetVersion()
    {
        var version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? "unknown";

        return version.Split('+')[0];
    }
}
