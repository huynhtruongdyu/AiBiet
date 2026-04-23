using System.Reflection;

using AiBiet.CLI.Infrastructure;
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

    }

    private static string GetVersion() => AppInfo.GetVersion();
}
