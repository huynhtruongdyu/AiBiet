using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using AiBiet.CLI.Infrastructure;

namespace AiBiet.CLI.Commands;

internal class ConfigSettings : CommandSettings
{
    [CommandArgument(0, "[provider]")]
    [Description("The AI provider to configure (e.g., ollama, openai, gemini)")]
    public string? Provider { get; set; }

    [CommandOption("--url")]
    [Description("Set the API model URL")]
    public string? Url { get; set; }

    [CommandOption("--key")]
    [Description("Set the API key")]
    public string? Key { get; set; }

    [CommandOption("--secret")]
    [Description("Set the secret key")]
    public string? Secret { get; set; }

    [CommandOption("--default")]
    [Description("Set this provider as the default")]
    public bool SetAsDefault { get; set; }
}

internal class ConfigCommand : AsyncCommand<ConfigSettings>
{
    private readonly AiBietConfig _config;

    public ConfigCommand(AiBietConfig config)
    {
        _config = config;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ConfigSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(settings.Provider))
        {
            ShowConfig();
            return 0;
        }

        var provider = settings.Provider.ToUpperInvariant();
        
        if (!_config.Providers.TryGetValue(provider, out var pConfig))
        {
            pConfig = new ProviderConfig();
            _config.Providers[provider] = pConfig;
        }

        bool changed = false;

        if (settings.Url != null)
        {
            pConfig.ApiUrl = settings.Url;
            changed = true;
        }

        if (settings.Key != null)
        {
            pConfig.ApiKey = settings.Key;
            changed = true;
        }

        if (settings.Secret != null)
        {
            pConfig.SecretKey = settings.Secret;
            changed = true;
        }

        if (settings.SetAsDefault)
        {
            _config.DefaultProvider = provider;
            changed = true;
        }

        // If no options were provided, run interactive setup
        if (settings.Url == null && settings.Key == null && settings.Secret == null && !settings.SetAsDefault)
        {
            AnsiConsole.MarkupLine($"[bold]Configuring provider:[/] [green]{provider}[/]");
            
            pConfig.ApiUrl = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter API URL:")
                    .DefaultValue(pConfig.ApiUrl ?? ""), cancellationToken).ConfigureAwait(false);
            
            pConfig.ApiKey = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter API Key:")
                    .DefaultValue(pConfig.ApiKey ?? "")
                    .AllowEmpty(), cancellationToken).ConfigureAwait(false);

            pConfig.SecretKey = await AnsiConsole.PromptAsync(
                new TextPrompt<string>("Enter Secret Key:")
                    .DefaultValue(pConfig.SecretKey ?? "")
                    .AllowEmpty(), cancellationToken).ConfigureAwait(false);

            if (await AnsiConsole.ConfirmAsync("Set as default provider?", string.Equals(provider, _config.DefaultProvider, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false))
            {
                _config.DefaultProvider = provider;
            }
            
            changed = true;
        }

        if (changed)
        {
            await ConfigBootstrapper.SaveAsync(_config).ConfigureAwait(false);
            AnsiConsole.MarkupLine($"[green]Configuration updated for {provider}![/]");
        }

        return 0;
    }

    private void ShowConfig()
    {
        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aibiet", "config.json");

        AnsiConsole.MarkupLine($"[bold]Configuration File:[/] {configPath}");
        AnsiConsole.MarkupLine($"[bold]Default Provider:[/] {_config.DefaultProvider}");
        AnsiConsole.WriteLine();

        if (_config.Providers != null && _config.Providers.Count > 0)
        {
            var table = new Table();

            table.AddColumn("Provider");
            table.AddColumn("ApiUrl");
            table.AddColumn("ApiKey");
            table.AddColumn("SecretKey");

            foreach (var kvp in _config.Providers)
            {
                var providerName = kvp.Key;
                var pConfig = kvp.Value;

                table.AddRow(
                    providerName == _config.DefaultProvider ? $"[green]{providerName}[/]" : providerName,
                    pConfig.ApiUrl ?? "[grey]<not set>[/]",
                    string.IsNullOrEmpty(pConfig.ApiKey) ? "[grey]<not set>[/]" : "********",
                    string.IsNullOrEmpty(pConfig.SecretKey) ? "[grey]<not set>[/]" : "********"
                );
            }

            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]No providers configured.[/]");
        }
    }
}
