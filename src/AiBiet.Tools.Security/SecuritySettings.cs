using System.ComponentModel;

using Spectre.Console.Cli;

namespace AiBiet.Tools.Security;

public class SecuritySettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("File or directory to scan (default: current directory)")]
    public string? Path { get; set; }

    [CommandOption("-s|--staged")]
    [Description("Scan staged git changes only")]
    public bool ScanStaged { get; set; }

    [CommandOption("-u|--unstaged")]
    [Description("Scan unstaged git changes")]
    public bool ScanUnstaged { get; set; }

    [CommandOption("--full")]
    [Description("Scan entire codebase")]
    public bool ScanFull { get; set; }

    [CommandOption("--severity")]
    [Description("Filter by severity: critical, high, medium, low")]
    public string? SeverityFilter { get; set; }

    [CommandOption("--format")]
    [Description("Output format: pretty (default), json")]
    public string Format { get; set; } = "pretty";
}
