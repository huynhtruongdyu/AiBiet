using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiBiet.CLI.Infrastructure;

internal static class ConfigBootstrapper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task SaveAsync(AiBietConfig config)
    {
        var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aibiet");
        var configPath = Path.Combine(configDir, "config.json");

        var json = JsonSerializer.Serialize(config, _jsonOptions);
        await File.WriteAllTextAsync(configPath, json).ConfigureAwait(false);
    }

    public static async Task<AiBietConfig> InitializeAsync()
    {
        var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aibiet");
        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        var configPath = Path.Combine(configDir, "config.json");
        var schemaPath = Path.Combine(configDir, "config.schema.json");

        var schemaContent = """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "title": "AiBiet Configuration",
          "type": "object",
          "properties": {
            "DefaultProvider": {
              "type": "string",
              "description": "The default AI provider to use",
              "enum": ["ollama", "openai", "gemini"]
            },
            "Providers": {
              "type": "object",
              "description": "Configuration for individual providers",
              "additionalProperties": {
                "type": "object",
                "properties": {
                  "ApiUrl": {
                    "type": "string",
                    "description": "The API model URL"
                  },
                  "ApiKey": {
                    "type": "string",
                    "description": "The API key for the provider"
                  },
                  "SecretKey": {
                    "type": "string",
                    "description": "The secret key for the provider"
                  }
                }
              }
            }
          },
          "additionalProperties": false
        }
        """;
        
        await File.WriteAllTextAsync(schemaPath, schemaContent).ConfigureAwait(false);

        if (!File.Exists(configPath))
        {
            var defaultConfig = """
            {
              "$schema": "./config.schema.json",
              "DefaultProvider": "ollama",
              "Providers": {
                "ollama": {
                  "ApiUrl": "http://localhost:11434"
                },
                "openai": {
                  "ApiUrl": "https://api.openai.com/v1",
                  "ApiKey": ""
                },
                "gemini": {
                  "ApiKey": ""
                }
              }
            }
            """;
            await File.WriteAllTextAsync(configPath, defaultConfig).ConfigureAwait(false);
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: true, reloadOnChange: true)
            .Build();

        return configuration.Get<AiBietConfig>() ?? new AiBietConfig();
    }
}
