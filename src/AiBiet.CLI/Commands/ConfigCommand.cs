using System.ComponentModel;


using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;
using AiBiet.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands;

internal class ConfigSettings : CommandSettings
{
    [CommandArgument(0, "[provider]")]
    [Description("The AI provider to configure (e.g., ollama, openai, gemini)")]
    public string? Provider { get; set; }

    [CommandOption("--clear")]
    [Description("Clear the configuration for the specified provider, or the entire config if no provider is specified")]
    public bool Clear { get; set; }
}

internal class ConfigCommand : AsyncCommand<ConfigSettings>
{
    private static string RedactKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return "[grey]<not set>[/]";
        return key.Length <= 8
            ? "********"
            : $"{key[..4]}********{key[^4..]}";
    }

    private readonly AiBietConfig _config;

    public ConfigCommand(AiBietConfig config)
    {
        _config = config;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ConfigSettings settings, CancellationToken cancellationToken)
    {
        if (settings.Clear && string.IsNullOrEmpty(settings.Provider))
        {
            if (await AnsiConsole.ConfirmAsync("Are you sure you want to [red]clear all[/] configurations?", false, cancellationToken).ConfigureAwait(false))
            {
                _config.Providers.Clear();
                _config.DefaultProvider = null;
                await ConfigBootstrapper.SaveAsync(_config).ConfigureAwait(false);
                AnsiConsole.MarkupLine("[green]All configurations cleared![/]");
            }
            return 0;
        }

        if (string.IsNullOrEmpty(settings.Provider))
        {
            ShowConfig();
            return 0;
        }

        var provider = settings.Provider!.ToUpperInvariant();
        var providerDisplay = Markup.Escape(provider);

        if (settings.Clear)
        {
            if (_config.Providers.Remove(provider))
            {
                if (string.Equals(provider, _config.DefaultProvider, StringComparison.OrdinalIgnoreCase))
                {
                    _config.DefaultProvider = "";
                }
                await ConfigBootstrapper.SaveAsync(_config).ConfigureAwait(false);
                 AnsiConsole.MarkupLine($"[green]Configuration for provider '{providerDisplay}' cleared![/]");
            }
            else
            {
                 AnsiConsole.MarkupLine($"[yellow]Provider '{providerDisplay}' not found in configuration.[/]");
            }
            return 0;
        }

        if (!_config.Providers.TryGetValue(provider, out var pConfig))
        {
            pConfig = new ProviderConfig();
            _config.Providers[provider] = pConfig;
        }

        AnsiConsole.MarkupLine($"[bold]Configuring provider:[/] [green]{providerDisplay}[/]");

        pConfig.ApiUrl = await AnsiConsole.PromptAsync(
            new TextPrompt<string>("Enter API URL:")
                .DefaultValue(pConfig.ApiUrl ?? ""), cancellationToken).ConfigureAwait(false);

        pConfig.ApiKey = await AnsiConsole.PromptAsync(
            new TextPrompt<string>("Enter API Key:")
                .DefaultValue(pConfig.ApiKey ?? "")
                .AllowEmpty()
                .Secret(), cancellationToken).ConfigureAwait(false);

        pConfig.SecretKey = await AnsiConsole.PromptAsync(
            new TextPrompt<string>("Enter Secret Key:")
                .DefaultValue(pConfig.SecretKey ?? "")
                .AllowEmpty()
                .Secret(), cancellationToken).ConfigureAwait(false);

        pConfig.DefaultModel = await AnsiConsole.PromptAsync(
            new TextPrompt<string>("Enter Default Model:")
                .DefaultValue(pConfig.DefaultModel ?? "")
                .AllowEmpty(), cancellationToken).ConfigureAwait(false);

        if (await AnsiConsole.ConfirmAsync("Set as default provider?", string.Equals(provider, _config.DefaultProvider, StringComparison.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false))
        {
            _config.DefaultProvider = provider;
        }

        await ConfigBootstrapper.SaveAsync(_config).ConfigureAwait(false);
        AnsiConsole.MarkupLine($"[green]Configuration updated for {providerDisplay}![/]");

        return 0;
    }

    private void ShowConfig()
    {
        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aibiet", "config.json");

        AnsiConsole.MarkupLine($"[bold]Configuration File:[/] {configPath}");

        if (string.IsNullOrWhiteSpace(_config.DefaultProvider))
        {
            AnsiConsole.MarkupLine("[bold]Default Provider:[/] [yellow]Not set[/]");
        }
         else
         {
             AnsiConsole.MarkupLine($"[bold]Default Provider:[/] {Markup.Escape(_config.DefaultProvider ?? "")}");
         }

        AnsiConsole.WriteLine();

        if (_config.Providers != null && _config.Providers.Count > 0)
        {
            var table = new Table();

            table.AddColumn("Provider");
            table.AddColumn("ApiUrl");
            table.AddColumn("ApiKey");
            table.AddColumn("SecretKey");
            table.AddColumn("DefaultModel");

             foreach (var kvp in _config.Providers)
             {
                 var providerName = kvp.Key;
                 var pConfig = kvp.Value;

                 table.AddRow(
                     providerName == _config.DefaultProvider ? $"[green]{Markup.Escape(providerName)}[/]" : Markup.Escape(providerName),
                     string.IsNullOrEmpty(pConfig.ApiUrl) ? "[grey]<not set>[/]" : Markup.Escape(pConfig.ApiUrl),
                     RedactKey(pConfig.ApiKey),
                     RedactKey(pConfig.SecretKey),
                     string.IsNullOrEmpty(pConfig.DefaultModel) ? "[grey]<not set>[/]" : Markup.Escape(pConfig.DefaultModel)
                 );
             }
            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]No providers configured.[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Tool Sources:");

        if (_config.ToolSources != null && _config.ToolSources.Count > 0)
        {
            var table = new Table();
            table.AddColumn("#");
            table.AddColumn("Source");

             for (int i = 0; i < _config.ToolSources.Count; i++)
             {
                 table.AddRow((i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), Markup.Escape(_config.ToolSources[i]));
             }
            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]No tool sources configured.[/]");
        }
    }
}
