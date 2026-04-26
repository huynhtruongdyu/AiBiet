using AiBiet.Core.Interfaces;

using Spectre.Console.Cli;

namespace AiBiet.CLI.Infrastructure;

internal class ToolCommandWrapper<TTool, TSettings>(IAiProvider aiProvider) : AsyncCommand<TSettings>
    where TTool : ITool<TSettings>, new()
    where TSettings : CommandSettings
{
    private readonly IAiProvider _aiProvider = aiProvider;

    protected override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        var tool = new TTool();
        tool.Initialize(new ToolContext
        {
            AiProvider = _aiProvider
        });

        return await tool.ExecuteAsync(settings, cancellationToken).ConfigureAwait(false);
    }
}
