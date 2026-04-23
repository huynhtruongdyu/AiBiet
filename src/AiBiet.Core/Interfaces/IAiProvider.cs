using AiBiet.Core.Domain.Models;

namespace AiBiet.Core.Interfaces;

public interface IAiProvider
{
    /// <summary>
    /// The unique identifier for the provider (e.g. "ollama", "openai", "gemini")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Sends a single prompt to the specified model and returns the response.
    /// </summary>
    Task<ChatResponse> AskAsync(string model, string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a full chat history to the specified model and returns the response.
    /// </summary>
    Task<ChatResponse> ChatAsync(string model, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
}
