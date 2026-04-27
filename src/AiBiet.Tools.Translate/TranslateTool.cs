using System.ComponentModel;

using AiBiet.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.Tools.Translate;

public class TranslateSettings : CommandSettings
{
    [CommandArgument(0, "[text]")]
    [Description("Text to translate")]
    public string? Text { get; set; }

    [CommandOption("-t|--to")]
    [Description("Target language (e.g., 'en', 'vi', 'ja')")]
    public string To { get; set; } = "en";

    [CommandOption("-f|--from")]
    [Description("Source language (default: 'auto')")]
    public string From { get; set; } = "auto";
}

public class TranslateTool : ITool<TranslateSettings>
{
    private ToolContext _context = null!;

    public string Name => "translate";

    public string Description => "Translate text using the configured AI provider";

    public void Initialize(ToolContext context)
    {
        _context = context;
    }

    public async Task<int> ExecuteAsync(TranslateSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var text = settings.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            AnsiConsole.MarkupLine("[yellow]No text provided for translation.[/]");
            return 0;
        }

        var fromLang = settings.From;
        if (string.Equals(fromLang, "auto", StringComparison.OrdinalIgnoreCase))
        {
            fromLang = "the detected language";
        }

        var prompt = $"Translate the following text from {fromLang} to {settings.To}. " +
                     "Return ONLY the translated text, no explanations, no conversation, no markdown code blocks unless the source text has them.\n\n" +
                     $"Text: {text}";

        string? translatedText = null;

        await AnsiConsole.Status()
            .StartAsync("Translating...", async _ =>
            {
                var response = await _context.AiProvider.AskAsync(
                    prompt,
                    null,
                    cancellationToken).ConfigureAwait(false);

                if (response.IsSuccess)
                {
                    translatedText = response.Content.Trim();
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Error during translation:[/] {Markup.Escape(response.ErrorMessage ?? "Unknown error")}");
                }
            }).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(translatedText))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(new Text(translatedText))
            {
                Header = new PanelHeader($"Translation ({settings.To})"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1, 1, 1),
                Expand = true
            });
        }

        return 0;
    }
}
