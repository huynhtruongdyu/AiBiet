using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using AiBiet.Core.Domain.Models;
using AiBiet.Core.Interfaces;

namespace AiBiet.Providers.Gemini;

public sealed class GeminiProvider : IAiProvider
{
    private const string FallbackBaseUrl = "https://generativelanguage.googleapis.com/v1beta";
    private const string FallbackModel = "gemini-flash-latest";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _defaultModel;

    public string Name => "gemini";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = "Value comes from JSON config as string")]
    public GeminiProvider(string apiKey, HttpClient? httpClient = null, string? baseUrl = null, string? defaultModel = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("Gemini API key is required.", nameof(apiKey));

        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        _baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? FallbackBaseUrl : baseUrl.TrimEnd('/');
        _defaultModel = string.IsNullOrWhiteSpace(defaultModel) ? FallbackModel : defaultModel;
    }

    /// <inheritdoc />
    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var effectiveModel = string.IsNullOrWhiteSpace(request.Model) ? _defaultModel : request.Model;
        var url = $"{_baseUrl}/models/{effectiveModel}:generateContent?key={_apiKey}";

        var requestBody = BuildRequestBody(request.Messages);
        using var response = await _httpClient
            .PostAsJsonAsync(url, requestBody, _jsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return ChatResponse.Failure($"Gemini API error {(int)response.StatusCode}: {error}");
        }

        var result = await response.Content
            .ReadFromJsonAsync<GeminiGenerateResponse>(_jsonOptions, cancellationToken)
            .ConfigureAwait(false);

        var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                   ?? string.Empty;

        return ChatResponse.Success(text, effectiveModel);
    }


    private static GeminiGenerateRequest BuildRequestBody(IEnumerable<ChatMessage> messages)
    {
        var contents = new List<GeminiContent>();
        string? systemInstruction = null;

        foreach (var msg in messages)
        {
            if (msg.Role == ChatRole.System)
            {
                // Gemini handles system instructions separately
                systemInstruction = msg.Content;
                continue;
            }

            var geminiRole = msg.Role == ChatRole.Assistant ? "model" : "user";
            contents.Add(new GeminiContent
            {
                Role = geminiRole,
                Parts = [new GeminiPart { Text = msg.Content }]
            });
        }

        return new GeminiGenerateRequest
        {
            SystemInstruction = systemInstruction is not null
                ? new GeminiContent { Parts = [new GeminiPart { Text = systemInstruction }] }
                : null,
            Contents = contents
        };
    }


    private sealed class GeminiGenerateRequest
    {
        [JsonPropertyName("system_instruction")]
        public GeminiContent? SystemInstruction { get; set; }

        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = [];
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private sealed class GeminiGenerateResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }
}
