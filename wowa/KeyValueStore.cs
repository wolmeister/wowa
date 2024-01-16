using System.Text.Json;
using System.Text.Json.Serialization;

namespace wowa;

internal class KeyValueEntry(List<string> key, string value) {
    [JsonPropertyName("key")] public List<string> Key { get; } = key;

    [JsonPropertyName("value")] public string Value { get; } = value;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<KeyValueEntry>))]
internal partial class SourceGenerationContext : JsonSerializerContext;

public class KeyValueStore {
    private readonly List<KeyValueEntry> _entries;
    private readonly string _path;

    public KeyValueStore(string path) {
        _path = path;

        // Init the store if the file does not exist yet
        if (File.Exists(path) == false) {
            var directory = Path.GetDirectoryName(path) ?? throw new Exception("Invalid path");
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, "[]");
            _entries = [];
        }
        else {
            var jsonString = File.ReadAllText(path);
            var entries = JsonSerializer.Deserialize(jsonString, typeof(List<KeyValueEntry>),
                SourceGenerationContext.Default) as List<KeyValueEntry>;
            _entries = entries ?? [];
        }
    }

    public string? Get(List<string> key) {
        var entry = _entries.FirstOrDefault(e => e.Key.SequenceEqual(key));
        return entry?.Value;
    }

    public List<string> GetByPrefix(List<string> prefix) {
        var filteredEntries = _entries.Where(e => e.Key.Take(prefix.Count).SequenceEqual(prefix));
        return filteredEntries.Select(e => e.Value).ToList();
    }

    public void Set(List<string> key, string value) {
        // Update the in-memory entries
        _entries.RemoveAll(e => e.Key.SequenceEqual(key));
        _entries.Add(new KeyValueEntry(key, value));

        // Save to disk
        var result = JsonSerializer.Serialize(_entries, typeof(List<KeyValueEntry>), SourceGenerationContext.Default);
        File.WriteAllText(_path, result);
    }
}