using System;

using AiBiet.CLI.UI;

namespace AiBiet.CLI.Bootstrap;

internal static class ArgumentProcessor
{
    public static string[] Normalize(string[] args)
    {
        if (args.Length == 1 &&
            (args[0] == "-v" || args[0] == "-V"))
        {
            return ["--version"];
        }

        if (args.Length == 0)
        {
            SplashScreen.Show();
            return ["-h"];
        }

        return args;
    }
}