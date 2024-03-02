using System.Text.Json.Serialization;
using wowa.Curse;

namespace wowa;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<KeyValueEntry>))]
[JsonSerializable(typeof(GithubRelease))]
[JsonSerializable(typeof(Addon))]
[JsonSerializable(typeof(List<Addon>))]
[JsonSerializable(typeof(SearchModsResponse))]
[JsonSerializable(typeof(ModFileResponse))]
[JsonSerializable(typeof(RemoteWeakAura))]
[JsonSerializable(typeof(List<RemoteWeakAura>))]
public partial class JsonContext : JsonSerializerContext;