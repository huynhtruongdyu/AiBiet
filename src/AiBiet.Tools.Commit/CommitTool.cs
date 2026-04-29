using AiBiet.Core.Interfaces;
using AiBiet.Core.Utilities;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.Tools.Commit;

public class CommitTool : ITool<CommitSettings>
{
    private ToolContext _context = null!;

    public string Name => "commit";
    public string Description => "Generate conventional commit message using AI";

    public void Initialize(ToolContext context)
    {
        _context = context;
    }

    public async Task<int> ExecuteAsync(CommitSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var diff = settings.IncludeUnstaged
            ? await GitService.GetUnstagedDiffAsync(cancellationToken).ConfigureAwait(false)
            : await GitService.GetStagedDiffAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(diff))
        {
            AnsiConsole.MarkupLine("[yellow]No changes detected. Stage changes with 'git add' first.[/]");
            return 0;
        }

        var truncatedDiff = TruncateDiff(diff, maxLines: 200);
        var prompt = BuildPrompt(truncatedDiff, settings.Scope);

        string? commitMessage = null;

        await AnsiConsole.Status()
            .StartAsync("Generating commit message...", async ctx =>
            {
                var response = await _context.AiProvider.AskAsync(
                    prompt,
                    null,
                    cancellationToken).ConfigureAwait(false);

                if (response.IsSuccess)
                {
                    commitMessage = CleanCommitMessage(response.Content);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Error during generation:[/] {Markup.Escape(response.ErrorMessage ?? "Unknown error")}");
                }
            }).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(commitMessage))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(new Text(commitMessage))
            {
                Header = new PanelHeader("Suggested Commit Message"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1, 1, 1),
                Expand = true
            });

            AnsiConsole.MarkupLine("\n[dim]Copy and use: git commit -m \"...\"[/]");
        }

        return 0;
    }

    private static string BuildPrompt(string diff, string? scope)
    {
        var scopeHint = !string.IsNullOrEmpty(scope) ? $" with scope '{scope}'" : "";
        return $@"Analyze this git diff and generate a Conventional Commit message{scopeHint}.

Rules:
- Use format: type(scope): description
- Types: feat, fix, chore, docs, style, refactor, test, perf
- Keep description under 50 chars
- Add body only if changes are complex
- Return ONLY the commit message, no quotes, no explanation

Diff:
```
{diff}
```";
    }

    private static string TruncateDiff(string diff, int maxLines)
    {
        var lines = diff.Split('\n');
        return lines.Length <= maxLines
            ? diff
            : string.Join('\n', lines.Take(maxLines)) + "\n... (truncated)";
    }

    private static string CleanCommitMessage(string content)
    {
        var message = content.Trim();

        if (message.StartsWith("```", StringComparison.Ordinal) && message.EndsWith("```", StringComparison.Ordinal))
        {
            message = message[3..^3].Trim();
        }

        return message;
    }
}
