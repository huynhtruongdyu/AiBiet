using AiBiet.Core.Domain.Models;

namespace AiBiet.Core.Interfaces;

public interface IAiProvider
{
    string Name { get; }

    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default);

    Task<ChatResponse> AskAsync(string prompt, string? model = null, CancellationToken cancellationToken = default)
        => ChatAsync(ChatRequest.FromPrompt(prompt, model), cancellationToken);
}
