using System.Text.Json;

namespace Veneer;

public class LanguageConfig
{
    public record Config(string[] imports, string[] libraries);


    public static Dictionary<string, Config>? DeserializeConfig(string path)
    {
        return File.Exists(path) 
            ? JsonSerializer.Deserialize<Dictionary<string, Config>>(File.ReadAllText(path)) 
            : null;
    }
}