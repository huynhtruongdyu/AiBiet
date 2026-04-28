using System.ComponentModel;

using Spectre.Console.Cli;

namespace AiBiet.Tools.Commit;

public class CommitSettings : CommandSettings
{
    [CommandOption("-a|--all")]
    [Description("Include unstaged changes (git diff)")]
    public bool IncludeUnstaged { get; set; }

    [CommandOption("-s|--staged")]
    [Description("Use staged changes only (default)")]
    public bool StagedOnly { get; set; } = true;

    [CommandOption("--scope")]
    [Description("Optional scope for conventional commit (e.g., 'auth', 'ui')")]
    public string? Scope { get; set; }
}
