using System.ComponentModel;
using System.Text.Json;

using AiBiet.CLI.Infrastructure;
using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;
using AiBiet.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands;

internal class ChatCommandSettings : CommandSettings
{
    [CommandOption("-m|--model")]
    [Description("Model name. Uses provider default if omitted.")]
    public string? Model { get; set; }

    [CommandOption("-s|--system")]
    [Description("System prompt / persona for the conversation")]
    public string? SystemPrompt { get; set; }

    [CommandOption("-f|--format")]
    [Description("Output format: plain, json, file")]
    public string? Format { get; set; }

    [CommandOption("--output|-o")]
    [Description("Output file path (used with --format file)")]
    public string? OutputPath { get; set; }
}

internal class ChatCommand : AsyncCommand<ChatCommandSettings>
{
    private readonly AiProviderResolver _resolver;
    private readonly AiBietConfig _config;

    public ChatCommand(AiProviderResolver resolver, AiBietConfig config)
    {
        _resolver = resolver;
        _config = config;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ChatCommandSettings settings, CancellationToken cancellationToken)
    {
        var model = settings.Model ?? string.Empty;
        var format = ParseOutputFormat(settings.Format);

        // Resolve provider at runtime (allows config changes to take effect)
        var provider = _resolver.Resolve(_config);

        AnsiConsole.Write(new Rule($"[bold cyan]Chat — {provider.Name}[/]").RuleStyle("cyan dim"));
        AnsiConsole.MarkupLine($"[dim]Model:[/] [cyan]{(string.IsNullOrEmpty(model) ? "default" : model)}[/]  [dim]Format:[/] [cyan]{format}[/]");
        AnsiConsole.MarkupLine("Type [yellow]exit[/] or press [yellow]Ctrl+C[/] to quit.\n");

        var history = new List<ChatMessage>();
        var allResponses = new List<string>();

        var systemPrompt = settings.SystemPrompt;
        if (string.IsNullOrWhiteSpace(systemPrompt) && format == OutputFormat.PlainText)
        {
            systemPrompt = "Avoid using Markdown formatting like headers, bold, italics, or code blocks. Provide your response in plain text only.";
        }

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            history.Add(ChatMessage.System(systemPrompt));
            if (!string.IsNullOrWhiteSpace(settings.SystemPrompt))
            {
                AnsiConsole.MarkupLine($"[dim italic]System:[/] {Markup.Escape(settings.SystemPrompt)}\n");
            }
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var input = await AnsiConsole.AskAsync<string>("[bold blue]You:[/]", cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            history.Add(ChatMessage.User(input));

            try
            {
                string responseText = string.Empty;

                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("cyan"))
                    .StartAsync("Thinking...", async _ =>
                    {
                        var request = new ChatRequest { Model = model };
                        foreach (var msg in history)
                        {
                            request.Messages.Add(msg);
                        }
                        var response = await provider.ChatAsync(request, cancellationToken).ConfigureAwait(false);
                        if (response.IsSuccess)
                        {
                            responseText = response.Content;
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"\n[red]AI Error:[/] {Markup.Escape(response.ErrorMessage ?? "Unknown error")}");
                            responseText = string.Empty;
                        }
                    }).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(responseText))
                {
                    history.Add(ChatMessage.Assistant(responseText));
                    allResponses.Add(responseText);
                }

                // Output based on format (except for file - handled at end)
                if (format != OutputFormat.File)
                {
                    AnsiConsole.WriteLine();
                    if (format == OutputFormat.Json)
                    {
                        var json = JsonSerializer.Serialize(new { response = responseText }, _jsonOptions);
                        AnsiConsole.WriteLine(json);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[bold green]AI:[/]");
                        AnsiConsole.WriteLine(responseText);
                    }
                    AnsiConsole.WriteLine();
                }
            }
            catch (OperationCanceledException)
            {
                break;
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
                // Don't exit — let the user try again or fix their network
            }
        }

        // Handle file output at the end of chat
        if (format == OutputFormat.File)
        {
            if (string.IsNullOrWhiteSpace(settings.OutputPath))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] --output/-o is required when using --format file");
            }
            else
            {
                var content = format == OutputFormat.Json
                    ? JsonSerializer.Serialize(new { responses = allResponses }, _jsonOptions)
                    : string.Join("\n\n---\n\n", allResponses);

                await File.WriteAllTextAsync(settings.OutputPath, content, cancellationToken).ConfigureAwait(false);
                AnsiConsole.MarkupLine($"[green]Chat saved to:[/] [cyan]{settings.OutputPath}[/]");
            }
        }

        AnsiConsole.MarkupLine("\n[dim]Chat session ended.[/]");
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
}