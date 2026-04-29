using AiBiet.Core.Interfaces;
using AiBiet.Core.Utilities;

using Spectre.Console;
using Spectre.Console.Cli;

using System.Text;

namespace AiBiet.Tools.Security;

public class SecurityTool : ITool<SecuritySettings>
{
    private ToolContext _context = null!;

    public string Name => "security";
    public string Description => "Scan code for security vulnerabilities using AI";

    public void Initialize(ToolContext context)
    {
        _context = context;
    }

    public async Task<int> ExecuteAsync(SecuritySettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var (code, source, fileCount) = await GetScanTargetAsync(settings, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(code))
        {
            AnsiConsole.MarkupLine("[yellow]No code found to scan.[/]");
            return 0;
        }

        var truncatedCode = TruncateCode(code, maxLines: 200);
        var prompt = BuildSecurityPrompt(truncatedCode, source);

        string? rawResult = null;

        await AnsiConsole.Status()
            .StartAsync("Scanning for security vulnerabilities...", async _ =>
            {
                var response = await _context.AiProvider.AskAsync(
                    prompt,
                    null,
                    cancellationToken).ConfigureAwait(false);

                if (response.IsSuccess)
                {
                    rawResult = response.Content.Trim();
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Error during scan:[/] {Markup.Escape(response.ErrorMessage ?? "Unknown error")}");
                }
            }).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(rawResult))
        {
            DisplayResults(rawResult, settings, fileCount);
        }

        return 0;
    }

    private static readonly string[] SourceExtensions =
    [
        // C# / .NET
        ".cs", ".vb", ".fs", ".csproj", ".vbproj", ".fsproj", ".sln",
        // Java / Kotlin / Scala
        ".java", ".kt", ".kts", ".scala", ".gradle", ".pom", ".xml",
        // JavaScript / TypeScript
        ".js", ".ts", ".jsx", ".tsx", ".mjs", ".cjs",
        // Web frameworks
        ".html", ".htm", ".css", ".scss", ".less", ".vue", ".svelte",
        // Python
        ".py", ".pyw", ".pyx", ".pxd", ".pxi",
        // Go
        ".go",
        // Rust
        ".rs",
        // C/C++
        ".c", ".cpp", ".cc", ".cxx", ".h", ".hpp", ".hxx",
        // Ruby
        ".rb", ".rake", ".gemspec",
        // PHP
        ".php",
        // Swift / Objective-C
        ".swift", ".m", ".mm",
        // Config files
        ".json", ".yaml", ".yml", ".toml", ".ini", ".conf", ".env",
        ".config", ".cfg", ".properties", ".env.example",
        // Shell
        ".sh", ".bash", ".zsh", ".ps1", ".bat", ".cmd"
    ];

    private static readonly string[] SkipDirectories =
    [
        "bin", "obj", "node_modules", "vendor", "packages", "dist", "build", "out", "release",
        ".git", ".svn", ".hg", ".idea", ".vscode", "target", "Cargo.lock",
        "__pycache__", "venv", ".venv", "env", ".env", "Pods", "DerivedData",
        "bower_components", ".next", ".nuxt", "coverage", ".turbo"
    ];

    private const long MaxFileSizeBytes = 500 * 1024; // 500KB
    private const int MaxDegreeOfParallelism = 5;

    private static async Task<(string code, string source, int fileCount)> GetScanTargetAsync(SecuritySettings settings, CancellationToken cancellationToken)
    {
        if (settings.ScanStaged)
        {
            var diff = await GitService.GetStagedDiffAsync(cancellationToken).ConfigureAwait(false);
            return (diff, "staged git changes", 0);
        }

        if (settings.ScanUnstaged)
        {
            var diff = await GitService.GetUnstagedDiffAsync(cancellationToken).ConfigureAwait(false);
            return (diff, "unstaged git changes", 0);
        }

        var targetPath = settings.Path ?? ".";
        if (Directory.Exists(targetPath))
        {
            var files = GetCodeFiles(targetPath, settings.ScanFull ? int.MaxValue : 30).ToList();

            if (files.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No source files found to scan.[/]");
                return (string.Empty, string.Empty, 0);
            }

            var fileCount = files.Count;
            AnsiConsole.MarkupLine($"[dim]Found {fileCount} file(s) to scan...[/]");

            // Read files in parallel with throttling
            var semaphore = new SemaphoreSlim(MaxDegreeOfParallelism);
            var readTasks = files.Select(async f =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var content = await File.ReadAllTextAsync(f, cancellationToken).ConfigureAwait(false);
                    return (filePath: f, content);
                }
                catch (UnauthorizedAccessException)
                {
                    AnsiConsole.MarkupLine($"[yellow]Warning: Cannot access {f}[/]");
                    return (filePath: f, content: string.Empty);
                }
                catch (IOException ex)
                {
                    AnsiConsole.MarkupLine($"[yellow]Warning: Error reading {f}: {ex.Message}[/]");
                    return (filePath: f, content: string.Empty);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            var results = await Task.WhenAll(readTasks).ConfigureAwait(false);
            semaphore.Dispose();

            // Build final string with StringBuilder for memory efficiency
            var codeBuilder = new StringBuilder();
            foreach (var (filePath, content) in results.Where(r => !string.IsNullOrEmpty(r.content)))
            {
                codeBuilder.Append("File: ").AppendLine(filePath);
                codeBuilder.AppendLine(content);
                codeBuilder.AppendLine("---");
            }

            return (codeBuilder.ToString(), $"directory: {targetPath}", fileCount);
        }

        if (File.Exists(targetPath))
        {
            var fileInfo = new FileInfo(targetPath);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping {targetPath}: file too large ({fileInfo.Length / 1024}KB > 500KB)[/]");
                return (string.Empty, string.Empty, 0);
            }

            var code = await File.ReadAllTextAsync(targetPath, cancellationToken).ConfigureAwait(false);
            return (code, $"file: {targetPath}", 1);
        }

        return (string.Empty, string.Empty, 0);
    }

    private static IEnumerable<string> GetCodeFiles(string directory, int maxFiles)
    {
        var gitIgnorePatterns = LoadGitIgnorePatterns(directory);
        var files = new List<string>();

        try
        {
            var allFiles = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var relativePath = Path.GetRelativePath(directory, f);
                    var extension = Path.GetExtension(f).ToLowerInvariant();

                    // Skip non-source files
                    if (!SourceExtensions.Contains(extension))
                        return false;

                    // Skip binary files
                    if (IsBinaryFile(f))
                        return false;

                    // Skip directories
                    if (SkipDirectories.Any(d =>
                        relativePath.Contains($"{Path.DirectorySeparatorChar}{d}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
                        relativePath.StartsWith($"{d}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
                        return false;

                    // Check .gitignore patterns
                    if (gitIgnorePatterns.Count > 0 && IsGitIgnored(relativePath, gitIgnorePatterns))
                        return false;

                    // Check file size
                    var fileInfo = new FileInfo(f);
                    if (fileInfo.Length > MaxFileSizeBytes)
                        return false;

                    return true;
                });

            // Prioritize important files first
            files = allFiles
                .OrderBy(f =>
                {
                    var name = Path.GetFileName(f).ToLowerInvariant();
                    return name switch
                    {
                        "package.json" or "cargo.toml" or "go.mod" or "pom.xml" or "build.gradle" or "requirements.txt" => 0,
                        "appsettings.json" or "config.json" or ".env" or "web.config" => 1,
                        _ when name.Contains("config", StringComparison.OrdinalIgnoreCase) => 2,
                        _ => 3
                    };
                })
                .Take(maxFiles)
                .ToList();
        }
        catch (UnauthorizedAccessException)
        {
            AnsiConsole.MarkupLine("[yellow]Warning: Some files could not be accessed due to permissions[/]");
        }

        return files;
    }

    private static bool IsBinaryFile(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var buffer = new byte[8192];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);

            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0)
                    return true;
            }
        }
        catch
        {
            return true;
        }

        return false;
    }

    private static List<string> LoadGitIgnorePatterns(string directory)
    {
        var patterns = new List<string>();
        var gitIgnorePath = Path.Combine(directory, ".gitignore");

        if (!File.Exists(gitIgnorePath))
            return patterns;

        try
        {
            var lines = File.ReadAllLines(gitIgnorePath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith('#'))
                    patterns.Add(trimmed);
            }
        }
        catch
        {
            // Ignore errors reading .gitignore
        }

        return patterns;
    }

    private static bool IsGitIgnored(string relativePath, List<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            if (pattern.EndsWith('/'))
            {
                if (relativePath.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) ||
                    relativePath.Contains($"/{pattern}", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (pattern.Contains('*', StringComparison.Ordinal) || pattern.Contains('?', StringComparison.Ordinal))
            {
                if (IsMatchWildcard(relativePath, pattern))
                    return true;
            }
            else if (string.Equals(relativePath, pattern, StringComparison.OrdinalIgnoreCase) ||
                     relativePath.EndsWith($"/{pattern}", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsMatchWildcard(string input, string pattern)
    {
        var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal).Replace("\\?", ".", StringComparison.Ordinal) + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(input, regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string BuildSecurityPrompt(string code, string source)
    {
        return $@"Analyze this code/diff for security vulnerabilities. Return each finding in this EXACT format:

SEVERITY: [critical|high|medium|low]
FILE: [path:line or '{source}']
ISSUE: [brief description]
FIX: [actionable suggestion]

Separate each finding with a line containing only '---'.

Scan for:
- OWASP Top 10 issues (injection, XSS, CSRF, etc.)
- Hardcoded secrets/credentials/API keys
- Insecure cryptographic practices
- Missing input validation
- Insecure dependencies (if package files are present)

Return ONLY the findings in the format above, no intro/outro text.

Code/Diff to scan:
```
{code}
```";
    }

    private static string TruncateCode(string code, int maxLines)
    {
        var lines = code.Split('\n');
        return lines.Length <= maxLines
            ? code
            : string.Join('\n', lines.Take(maxLines)) + "\n... (truncated)";
    }

    private static void DisplayResults(string rawResult, SecuritySettings settings, int fileCount)
    {
        if (string.Equals(settings.Format, "json", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.WriteLine(rawResult);
            return;
        }

        var findings = ParseFindings(rawResult);

        if (findings.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]No security issues found![/]");
            return;
        }

        // Sort by severity: critical > high > medium > low
        var severityOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["critical"] = 0,
            ["high"] = 1,
            ["medium"] = 2,
            ["low"] = 3
        };

        var sortedFindings = findings
            .OrderBy(f => severityOrder.TryGetValue(f.Severity, out var order) ? order : 4)
            .ToList();

        AnsiConsole.WriteLine();
        var table = new Table
        {
            Title = new TableTitle("Security Scan Results"),
            Border = TableBorder.Rounded,
            Expand = true
        };

        table.AddColumn(new TableColumn("Severity") { Width = 10 });
        table.AddColumn(new TableColumn("File") { Width = 50 });
        table.AddColumn(new TableColumn("Issue") { Width = 60 });
        table.AddColumn(new TableColumn("Fix") { Width = 70 });

        foreach (var finding in sortedFindings)
        {
            var severityMarkup = finding.Severity.ToLowerInvariant() switch
            {
                "critical" => "[red bold]CRITICAL[/]",
                "high" => "[red]HIGH[/]",
                "medium" => "[yellow]MEDIUM[/]",
                "low" => "[green]LOW[/]",
                _ => $"[white]{finding.Severity}[/]"
            };

            table.AddRow(
                severityMarkup,
                Markup.Escape(finding.File),
                Markup.Escape(finding.Issue),
                Markup.Escape(finding.Fix)
            );

            table.AddEmptyRow();
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Scanned {fileCount} file(s), found {sortedFindings.Count} finding(s)[/]");
    }

    private static List<Finding> ParseFindings(string rawResult)
    {
        var findings = new List<Finding>();
        var blocks = rawResult.Split("---", StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in blocks)
        {
            var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var finding = new Finding();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("SEVERITY:", StringComparison.OrdinalIgnoreCase))
                    finding.Severity = trimmed["SEVERITY:".Length..].Trim();
                else if (trimmed.StartsWith("FILE:", StringComparison.OrdinalIgnoreCase))
                    finding.File = trimmed["FILE:".Length..].Trim();
                else if (trimmed.StartsWith("ISSUE:", StringComparison.OrdinalIgnoreCase))
                    finding.Issue = trimmed["ISSUE:".Length..].Trim();
                else if (trimmed.StartsWith("FIX:", StringComparison.OrdinalIgnoreCase))
                    finding.Fix = trimmed["FIX:".Length..].Trim();
            }

            if (!string.IsNullOrEmpty(finding.Severity) || !string.IsNullOrEmpty(finding.Issue))
                findings.Add(finding);
        }

        // If no blocks were found (no "---" separator), try parsing as single finding
        if (findings.Count == 0 && !string.IsNullOrWhiteSpace(rawResult))
        {
            var finding = new Finding();
            var lines = rawResult.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("SEVERITY:", StringComparison.OrdinalIgnoreCase))
                    finding.Severity = trimmed["SEVERITY:".Length..].Trim();
                else if (trimmed.StartsWith("FILE:", StringComparison.OrdinalIgnoreCase))
                    finding.File = trimmed["FILE:".Length..].Trim();
                else if (trimmed.StartsWith("ISSUE:", StringComparison.OrdinalIgnoreCase))
                    finding.Issue = trimmed["ISSUE:".Length..].Trim();
                else if (trimmed.StartsWith("FIX:", StringComparison.OrdinalIgnoreCase))
                    finding.Fix = trimmed["FIX:".Length..].Trim();
            }

            if (!string.IsNullOrEmpty(finding.Severity) || !string.IsNullOrEmpty(finding.Issue))
                findings.Add(finding);
        }

        return findings;
    }

    private sealed class Finding
    {
        public string Severity { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public string Issue { get; set; } = string.Empty;
        public string Fix { get; set; } = string.Empty;
    }
}
