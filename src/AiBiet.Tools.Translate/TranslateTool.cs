using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using AiBiet.Core.Interfaces;

using Spectre.Console.Cli;

namespace AiBiet.Tools.Translate;

public class TranslateSettings : CommandSettings
{
    [CommandArgument(0, "[text]")]
    [Description("Text to translate")]
    public string? Text { get; set; }

    [CommandOption("-t|--to")]
    [Description("Target language")]
    public string To { get; set; } = "en";

    [CommandOption("-f|--from")]
    [Description("Source language")]
    public string From { get; set; } = "auto";

    [CommandOption("-h|--help")]
    public bool Help { get; set; }
}

public class TranslateTool : ITool<TranslateSettings>
{
    private ToolContext _context = null!;

    public string Name => "translate";

    public string Description => "Translate text using AI";

    public Task<int> ExecuteAsync(TranslateSettings settings, CancellationToken cancellationToken = default)
    {
        System.Console.WriteLine(settings?.Text);
        return Task.FromResult(0);
    }

    public void Initialize(ToolContext context)
    {
        _context = context;
    }

}
