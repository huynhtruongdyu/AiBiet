using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace AiBiet.Core.Domain.Models;

public class AiBietConfig
{
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "./config.schema.json";

    public string? DefaultProvider { get; set; }
    
    public Dictionary<string, ProviderConfig> Providers { get; } = new(StringComparer.OrdinalIgnoreCase);
    
    public Collection<string> ToolSources { get; } = [];
    
    [JsonIgnore]
    public string ToolsPath { get; set; } = "";
}

public class ProviderConfig
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Value comes from JSON config as string")]
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public string? DefaultModel { get; set; }
}
