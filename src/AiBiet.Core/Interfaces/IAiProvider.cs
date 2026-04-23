using AiBiet.Core.Domain.Models;

namespace AiBiet.Core.Interfaces;

public interface IAiProvider
{
    string Name { get; }

    Task<ChatResponse> AskAsync(string model, string prompt, CancellationToken cancellationToken = default);

    Task<ChatResponse> ChatAsync(string model, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
}
