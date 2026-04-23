using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

namespace AiBiet.CLI.Infrastructure;

internal sealed class NoProviderConfigured : IAiProvider
{
    private readonly string _configuredProvider;

    public string Name => "none";

    public NoProviderConfigured(string configuredProvider)
    {
        _configuredProvider = configuredProvider;
    }

    public Task<ChatResponse> AskAsync(string model, string prompt, CancellationToken cancellationToken = default)
        => Fail();

    public Task<ChatResponse> ChatAsync(string model, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        => Fail();

    private Task<ChatResponse> Fail()
    {
        throw new InvalidOperationException(
            $"No provider is ready. Your default provider is '{_configuredProvider}' but its API key is missing. " +
            $"Run 'aibiet config {_configuredProvider}' to set it up.");
    }
}
