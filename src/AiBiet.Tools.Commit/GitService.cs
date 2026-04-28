using System.Diagnostics;

namespace AiBiet.Tools.Commit;

public static class GitService
{
    public static async Task<string> GetStagedDiffAsync(CancellationToken cancellationToken = default)
        => await RunGitCommand("diff --cached", cancellationToken).ConfigureAwait(false);

    public static async Task<string> GetUnstagedDiffAsync(CancellationToken cancellationToken = default)
        => await RunGitCommand("diff", cancellationToken).ConfigureAwait(false);

    public static async Task<bool> HasStagedChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await RunGitCommand("diff --cached --quiet 2>&1 || echo 'HAS_CHANGES'", cancellationToken).ConfigureAwait(false);
        return result.Contains("HAS_CHANGES", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(result);
    }

    private static async Task<string> RunGitCommand(string arguments, CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        _ = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return output;
    }
}
