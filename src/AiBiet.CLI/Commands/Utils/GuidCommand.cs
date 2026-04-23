using System.ComponentModel;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AiBiet.CLI.Commands.Utils;

internal class GuidCommandSettings : CommandSettings
{
    [CommandOption("-c|--count")]
    [DefaultValue(1)]
    [Description("Number of GUIDs to generate")]
    public int Count { get; set; }

    [CommandOption("-u|--uppercase")]
    [DefaultValue(false)]
    [Description("Output in uppercase")]
    public bool Uppercase { get; set; }

    [CommandOption("-n|--no-dashes")]
    [DefaultValue(false)]
    [Description("Output without dashes")]
    public bool NoDashes { get; set; }

    [CommandOption("-b|--braces")]
    [DefaultValue(false)]
    [Description("Output enclosed in curly braces")]
    public bool Braces { get; set; }
}

internal class GuidCommand : Command<GuidCommandSettings>
{
    protected override int Execute(CommandContext context, GuidCommandSettings settings, CancellationToken cancellationToken)
    {
        var format = settings.NoDashes ? "N" : "D";
        if (settings.Braces && !settings.NoDashes)
        {
            format = "B";
        }

        for (int i = 0; i < settings.Count; i++)
        {
            var guidString = Guid.NewGuid().ToString(format);
            if (settings.Uppercase)
            {
                guidString = guidString.ToUpperInvariant();
            }
            if (settings.Braces && settings.NoDashes)
            {
                guidString = $"{{{guidString}}}";
            }

            AnsiConsole.MarkupLine($"[green]{guidString}[/]");
        }

        return 0;
    }
}
