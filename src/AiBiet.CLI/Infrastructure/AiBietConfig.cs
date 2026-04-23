using System.Text.Json.Serialization;

namespace AiBiet.CLI.Infrastructure;

internal class AiBietConfig
{
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "./config.schema.json";

    public string DefaultProvider { get; set; } = "ollama";
    public Dictionary<string, ProviderConfig> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal class ProviderConfig
{
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
}
