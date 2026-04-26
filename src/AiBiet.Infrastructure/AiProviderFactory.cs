using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;
using AiBiet.Providers.Gemini;

using Microsoft.Extensions.DependencyInjection;

namespace AiBiet.Infrastructure;

public class AiProviderFactory(AiBietConfig config, IHttpClientFactory httpClientFactory) : IAiProviderFactory
{
    private readonly AiBietConfig _config = config;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public IAiProvider GetDefaultProvider()
    {
        if (string.IsNullOrWhiteSpace(_config.DefaultProvider))
        {
            return new NullAiProvider("No default provider configured.");
        }

        return GetProvider(_config.DefaultProvider);
    }

    public IAiProvider GetProvider(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var providerName = name.ToUpperInvariant();

        if (!_config.Providers.TryGetValue(providerName, out var providerConfig))
        {
            return new NullAiProvider($"Provider '{name}' not found in configuration.");
        }

        return providerName switch
        {
            "GEMINI" => CreateGeminiProvider(providerConfig),
            _ => new NullAiProvider($"Provider '{name}' is not supported yet.")
        };
    }

    private GeminiProvider CreateGeminiProvider(ProviderConfig providerConfig)
    {
        return new GeminiProvider(
            providerConfig.ApiKey ?? throw new InvalidOperationException("Gemini API key is missing."),
            _httpClientFactory.CreateClient("gemini"),
            providerConfig.ApiUrl,
            providerConfig.DefaultModel);
    }
}

internal class NullAiProvider(string message) : IAiProvider
{
    private readonly string _message = message;

    public string Name => "null";

    public Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ChatResponse.Failure(_message));
    }
}
