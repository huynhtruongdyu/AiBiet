using Microsoft.Extensions.Configuration;

namespace AiBiet.CLI.Infrastructure;

internal static class ConfigBootstrapper
{
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
            }
          },
          "additionalProperties": false
        }
        """;
        
        await File.WriteAllTextAsync(schemaPath, schemaContent).ConfigureAwait(false);

        if (!File.Exists(configPath))
        {
            await File.WriteAllTextAsync(configPath, "{\n  \"$schema\": \"./config.schema.json\",\n  \"DefaultProvider\": \"ollama\"\n}").ConfigureAwait(false);
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: true, reloadOnChange: true)
            .Build();

        return configuration.Get<AiBietConfig>() ?? new AiBietConfig();
    }
}
