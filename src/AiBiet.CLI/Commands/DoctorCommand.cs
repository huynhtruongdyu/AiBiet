using System.ComponentModel;
using AiBiet.CLI.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands;

internal class DoctorSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [Description("Show detailed information about each check")]
    public bool Verbose { get; set; }
}

internal class DoctorCommand : AsyncCommand<DoctorSettings>
{
    private readonly AiBietConfig _config;
    private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    public DoctorCommand(AiBietConfig config)
    {
        _config = config;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, DoctorSettings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new FigletText("DOCTOR").Color(Color.Green));
        AnsiConsole.MarkupLine("[bold blue]AiBiet System Health Check[/]");
        AnsiConsole.WriteLine();

        // 1. Config Check
        await RunCheck("Configuration File", CheckConfig, cancellationToken).ConfigureAwait(false);

        // 2. Internet Check
        await RunCheck("Internet Connectivity", CheckInternetAsync, cancellationToken).ConfigureAwait(false);

        // 3. Ollama Check
        await RunCheck("Ollama Service", CheckOllamaAsync, cancellationToken).ConfigureAwait(false);

        // 4. Other Providers
        foreach (var provider in _config.Providers)
        {
            if (provider.Key.Equals("ollama", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await RunCheck($"Provider: {provider.Key}",
                () => CheckProviderAsync(provider.Key, provider.Value), cancellationToken).ConfigureAwait(false);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold green]All checks completed![/] If you have issues, try running [yellow]aibiet config[/] to update your settings.");

        return 0;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Safety catch for health checks")]
    private static async Task RunCheck(string name, Func<Task<(bool success, string message)>> check, CancellationToken ct)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"Checking {name}...", async _ =>
            {
                try
                {
                    var (success, message) = await check().ConfigureAwait(false);
                    if (success)
                    {
                        AnsiConsole.MarkupLine($"[green]✔[/] [bold]{name}:[/] {message}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]✘[/] [bold]{name}:[/] {message}");
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    AnsiConsole.MarkupLine($"[yellow]![/] [bold]{name}:[/] Check cancelled.");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]✘[/] [bold]{name}:[/] Unexpected error: {ex.Message}");
                }
            }).ConfigureAwait(false);
    }

    private static Task<(bool success, string message)> CheckConfig()
    {
        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aibiet", "config.json");
        if (File.Exists(configPath))
        {
            return Task.FromResult((true, $"Found config at [grey]{configPath}[/]"));
        }
        return Task.FromResult((false, "Configuration file missing. Run any command to generate it."));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Safety catch for health checks")]
    private static async Task<(bool success, string message)> CheckInternetAsync()
    {
        try
        {
            // Using a reliable endpoint for connectivity check
            using var response = await HttpClient.GetAsync(new Uri("https://www.google.com"), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            return (response.IsSuccessStatusCode, "Online and reachable.");
        }
        catch (HttpRequestException)
        {
            return (false, "Offline or google.com is unreachable.");
        }
        catch (Exception ex)
        {
            return (false, $"Connectivity check failed: {ex.Message}");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Safety catch for health checks")]
    private async Task<(bool success, string message)> CheckOllamaAsync()
    {
        var url = _config.Providers.TryGetValue("ollama", out var ollama) ? ollama.ApiUrl : "http://localhost:11434";
        url ??= "http://localhost:11434";

        try
        {
            using var response = await HttpClient.GetAsync(new Uri($"{url.TrimEnd('/')}/api/tags")).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return (true, $"Running at [blue]{url}[/]");
            }
            return (false, $"Ollama responded with error [yellow]{response.StatusCode}[/] at {url}");
        }
        catch (HttpRequestException)
        {
            return (false, $"Ollama not found at {url}. Make sure Ollama is running.");
        }
        catch (Exception ex)
        {
            return (false, $"Ollama check failed: {ex.Message}");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Safety catch for health checks")]
    private static async Task<(bool success, string message)> CheckProviderAsync(string name, ProviderConfig pConfig)
    {
        if (string.IsNullOrEmpty(pConfig.ApiUrl) && !name.Equals("gemini", StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"[yellow]API URL is not set.[/] Run [yellow]aibiet config {name}[/]");
        }

        if (name.Equals("gemini", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(pConfig.ApiKey))
            {
                return (false, "[yellow]API Key is missing.[/] Run [yellow]aibiet config gemini[/]");
            }

            return (true, "Configured (Ready for API calls)");
        }

        try
        {
            // Simple ping to the API URL
            using var response = await HttpClient.GetAsync(new Uri(pConfig.ApiUrl!), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            return (true, $"API endpoint [blue]{pConfig.ApiUrl}[/] is reachable.");
        }
        catch (HttpRequestException)
        {
            return (false, $"Could not reach API endpoint at {pConfig.ApiUrl}");
        }
        catch (Exception ex)
        {
            return (false, $"Provider check failed: {ex.Message}");
        }
    }
}
