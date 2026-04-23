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
