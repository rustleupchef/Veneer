using System.Text.Json;

namespace Veneer;

public static class LanguageConfig
{
    public record Config(string[] Imports, string[] Libraries);


    public static Dictionary<string, Config>? DeserializeConfig(string? path)
    {
        return File.Exists(path) 
            ? JsonSerializer.Deserialize<Dictionary<string, Config>>(File.ReadAllText(path)) 
            : null;
    }
}