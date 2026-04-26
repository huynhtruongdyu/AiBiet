using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;


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
                  },
                  "DefaultModel": {
                    "type": "string",
                    "description": "The default model to use for this provider"
                  }
                }
              }
            },
            "ToolSources": {
              "type": "array",
              "description": "Paths or URLs to load tools from",
              "items": {
                "type": "string"
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
              "Providers": {
                "ollama": {
                  "ApiUrl": "",
                  "DefaultModel": ""
                },
                "openai": {
                  "ApiUrl": "",
                  "ApiKey": "",
                  "DefaultModel": ""
                },
                "gemini": {
                  "ApiUrl": "",
                  "ApiKey": "",
                  "DefaultModel": ""
                }
              },
              "ToolSources": ["D:\\Projects\\github\\huynhtruongdyu\\AiBiet\\packages"]
            }
            """;
            await File.WriteAllTextAsync(configPath, defaultConfig).ConfigureAwait(false);
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: true, reloadOnChange: true)
            .Build();

        var config = configuration.Get<AiBietConfig>() ?? new AiBietConfig();

        // Ensure ToolsPath is set and exists
        if (string.IsNullOrWhiteSpace(config.ToolsPath))
        {
            config.ToolsPath = Path.Combine(configDir, "tools");
        }

        if (!Directory.Exists(config.ToolsPath))
        {
            Directory.CreateDirectory(config.ToolsPath);
        }

        // Ensure ToolsPath is in ToolSources
        if (!config.ToolSources.Contains(config.ToolsPath))
        {
            config.ToolSources.Add(config.ToolsPath);
        }

        return config;
    }
}
