using AiBiet.Core.Interfaces;
using AiBiet.Providers.Gemini;

namespace AiBiet.CLI.Infrastructure;

internal sealed class AiProviderResolver
{
    private readonly IHttpClientFactory _httpFactory;

    public AiProviderResolver(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    public IAiProvider Resolve(AiBietConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.DefaultProvider))
        {
            return new NoProviderConfigured("No default provider configured. Run 'aibiet config' to set one.");
        }

        var providerName = config.DefaultProvider.ToUpperInvariant();

        if (!config.Providers.TryGetValue(providerName, out var providerConfig))
        {
            return new NoProviderConfigured($"Provider '{config.DefaultProvider}' not found. Run 'aibiet config' to configure it.");
        }

        return providerName switch
        {
            "GEMINI" when !string.IsNullOrWhiteSpace(providerConfig.ApiKey) =>
                CreateGeminiProvider(providerConfig),

            "OLLAMA" => CreateOllamaProvider(),
            "OPENAI" when !string.IsNullOrWhiteSpace(providerConfig.ApiKey) =>
                CreateOpenAiProvider(),

            _ => new NoProviderConfigured(providerName)
        };
    }

    private GeminiProvider CreateGeminiProvider(ProviderConfig config)
    {
        return new GeminiProvider(
            config.ApiKey!,
            _httpFactory.CreateClient("gemini"),
            config.ApiUrl,
            config.DefaultModel);
    }

    private static IAiProvider CreateOllamaProvider()
    {
        // Ollama provider implementation pending
        return new NoProviderConfigured("ollama");
    }

    private static IAiProvider CreateOpenAiProvider()
    {
        // OpenAI provider implementation pending
        return new NoProviderConfigured("openai");
    }
}
