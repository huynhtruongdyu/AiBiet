using System.Text.Json.Serialization;

namespace AiBiet.CLI.Infrastructure;

internal class AiBietConfig
{
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "./config.schema.json";

    public string? DefaultProvider { get; set; }
    public Dictionary<string, ProviderConfig> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ToolSources { get; set; } = [];
    [JsonIgnore]
    public string ToolsPath { get; set; } = "";
}

internal class ProviderConfig
{
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public string? DefaultModel { get; set; }
}
