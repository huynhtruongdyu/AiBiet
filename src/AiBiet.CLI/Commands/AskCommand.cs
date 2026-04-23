using System.ComponentModel;
using System.Text.Json;

using Spectre.Console;
using Spectre.Console.Cli;

using AiBiet.CLI.Infrastructure;

namespace AiBiet.CLI.Commands;

internal enum OutputFormat
{
    PlainText,
    Json,
    File
}

internal class AskCommandSettings : CommandSettings
{
    [CommandOption("-m|--model")]
    [Description("Model name. Uses provider default if omitted.")]
    public string? Model { get; set; }

    [CommandArgument(0, "[prompt]")]
    [Description("The question or prompt to send to the AI")]
    public string Prompt { get; set; } = string.Empty;

    [CommandOption("-p|--provider")]
    [Description("Override the default AI provider (e.g. gemini, ollama, openai)")]
    public string? Provider { get; set; }

    [CommandOption("-f|--format")]
    [Description("Output format: plain, json, file")]
    public string? Format { get; set; }

    [CommandOption("--output|-o")]
    [Description("Output file path (used with --format file)")]
    public string? OutputPath { get; set; }
}

internal class AskCommand : AsyncCommand<AskCommandSettings>
{
    private readonly AiProviderResolver _resolver;
    private readonly AiBietConfig _config;

    public AskCommand(AiProviderResolver resolver, AiBietConfig config)
    {
        _resolver = resolver;
        _config = config;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, AskCommandSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Prompt))
        {
            settings.Prompt = await AnsiConsole.AskAsync<string>("[bold blue]>[/] Enter your prompt:", cancellationToken).ConfigureAwait(false);
        }

        // Resolve provider at runtime (allows config changes to take effect)
        var provider = _resolver.Resolve(_config);
        var model = settings.Model ?? string.Empty;

        AnsiConsole.MarkupLine($"[dim]Provider:[/] [cyan]{provider.Name}[/]  [dim]Model:[/] [cyan]{(string.IsNullOrEmpty(model) ? "default" : model)}[/]");
        AnsiConsole.WriteLine();

        try
        {
            string responseText = string.Empty;

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("Thinking...", async _ =>
                {
                    var response = await provider.AskAsync(model, settings.Prompt, cancellationToken).ConfigureAwait(false);
                    responseText = response.Content;
                }).ConfigureAwait(false);

            // Handle output format
            var format = ParseOutputFormat(settings.Format);
            await OutputResponse(responseText, format, settings.OutputPath, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]Configuration error:[/] {ex.Message}");
            AnsiConsole.MarkupLine("Run [yellow]aibiet config[/] to set up a provider.");
            return 1;
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine($"[red]API error:[/] {ex.Message}");
            return 1;
        }

        return 0;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private static OutputFormat ParseOutputFormat(string? format)
    {
        return format?.ToUpperInvariant() switch
        {
            "JSON" => OutputFormat.Json,
            "FILE" => OutputFormat.File,
            _ => OutputFormat.PlainText
        };
    }

    private static async Task OutputResponse(string content, OutputFormat format, string? outputPath, CancellationToken cancellationToken)
    {
        switch (format)
        {
            case OutputFormat.Json:
                var json = JsonSerializer.Serialize(new { response = content }, _jsonOptions);
                await WriteOutput(json, outputPath, cancellationToken).ConfigureAwait(false);
                break;

            case OutputFormat.File:
                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] --output/-o is required when using --format file");
                    return;
                }
                await File.WriteAllTextAsync(outputPath, content, cancellationToken).ConfigureAwait(false);
                AnsiConsole.MarkupLine($"[green]Response saved to:[/] [cyan]{outputPath}[/]");
                break;

            default:
                AnsiConsole.MarkupLine("[bold green]Response:[/]");
                await WriteOutput(content, outputPath, cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    private static async Task WriteOutput(string content, string? outputPath, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            await File.WriteAllTextAsync(outputPath, content, cancellationToken).ConfigureAwait(false);
            AnsiConsole.MarkupLine($"[green]Response saved to:[/] [cyan]{outputPath}[/]");
        }
        else
        {
            AnsiConsole.WriteLine(content);
        }
    }
}