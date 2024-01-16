using System.Text.Json;
using System.Text.Json.Serialization;

namespace wowa;

internal class GameVersionJsonConverter : JsonConverter<GameVersion> {
    public override GameVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String) {
            var enumString = reader.GetString();
            return enumString switch {
                "retail" => GameVersion.Retail,
                "classic" => GameVersion.Classic,
                _ => throw new JsonException($"Unable to parse {enumString} to enum type {typeof(GameVersion)}")
            };
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, GameVersion value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString().ToLower());
    }
}

[JsonConverter(typeof(GameVersionJsonConverter))]
public enum GameVersion {
    Classic,
    Retail
}

public class Addon {
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Author { get; init; }
    public required string Version { get; init; }
    public required GameVersion GameVersion { get; init; }
    public required List<string> Directories { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Addon))]
[JsonSerializable(typeof(List<Addon>))]
internal partial class AddonSourceGenerationContext : JsonSerializerContext;

public class AddonRepository(KeyValueStore store) {
    public void SaveAddon(Addon addon) {
        var serializedAddon = JsonSerializer.Serialize(addon, AddonSourceGenerationContext.Default.Addon);
        store.Set(["addons", addon.GameVersion.ToString().ToLower(), addon.Id], serializedAddon);
    }

    public void DeleteAddon(string id) {
        // TODO - Implement in the KV Store   
    }

    public Addon? GetAddon(string id, GameVersion gameVersion) {
        var serializedAddon = store.Get(["addons", gameVersion.ToString().ToLower(), id]);
        if (serializedAddon == null) return null;
        return DeserializeAddon(serializedAddon);
    }

    public List<Addon> GetAddons(GameVersion? gameVersion) {
        List<string> key = gameVersion != null
            ? ["addons", gameVersion.ToString()?.ToLower() ?? string.Empty]
            : ["addons"];
        var serializedAddons = store.GetByPrefix(key);
        return serializedAddons.ConvertAll(DeserializeAddon);
    }

    private static Addon DeserializeAddon(string serializedAddon) {
        var addon = JsonSerializer.Deserialize(serializedAddon, typeof(Addon),
            AddonSourceGenerationContext.Default) as Addon;
        return addon ?? throw new Exception("Failed to parse addon");
    }
}