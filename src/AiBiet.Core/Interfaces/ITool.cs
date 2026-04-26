using Spectre.Console.Cli;

namespace AiBiet.Core.Interfaces;

public interface ITool<in TSettings>
    where TSettings : CommandSettings
{
    string Name { get; }
    string Description { get; }
    void Initialize(ToolContext context);
    Task<int> ExecuteAsync(TSettings settings, CancellationToken cancellationToken = default);
}

public class ToolContext
{
    public required IAiProvider AiProvider { get; init; }
}
