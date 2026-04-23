using System.Reflection;

namespace AiBiet.CLI.Infrastructure;

internal static class AppInfo
{
    public static string GetVersion()
    {
        var version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? "unknown";

        return version.Split('+')[0];
    }
}
