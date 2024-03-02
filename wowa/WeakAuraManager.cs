using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;

namespace wowa;

internal class ChangelogFormatJsonConverter : JsonConverter<ChangelogFormat> {
    public override ChangelogFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String) {
            var enumString = reader.GetString();
            return enumString switch {
                "bbcode" => ChangelogFormat.Bbcode,
                "markdown" => ChangelogFormat.Markdown,
                _ => throw new JsonException($"Unable to parse {enumString} to enum type {typeof(ChangelogFormat)}")
            };
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, ChangelogFormat value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString().ToLower());
    }
}

[JsonConverter(typeof(ChangelogFormatJsonConverter))]
public enum ChangelogFormat {
    Bbcode,
    Markdown
}

public class Changelog {
    public string? Text { get; init; }
    public ChangelogFormat? Format { get; init; }
}

internal class RemoteWeakAura {
    [JsonPropertyName("_id")] public required string Id { get; init; }

    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string Url { get; init; }
    public required string Created { get; init; }
    public required string Modified { get; init; }
    public required string Type { get; init; }
    public required string Game { get; init; }
    public required string Thumbnail { get; init; }
    public required string Username { get; init; }
    public required int Version { get; init; }
    public required string VersionString { get; init; }
    public required Changelog? Changelog { get; init; }
}

public class LocalWeakAura {
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required int Version { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<RemoteWeakAura>))]
internal partial class WeakAuraGenerationContext : JsonSerializerContext;

public class WeakAuraUpdate {
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string Author { get; init; }
    public required string Encoded { get; init; }
    public required int WagoVersion { get; init; }
    public required string WagoSemver { get; init; }
    public required string Source { get; init; }
    public required Changelog? Changelog { get; init; }
}

public partial class WeakAuraManager(string gameFolder) {
    [GeneratedRegex(@"^https:\/\/wago\.io\/([a-zA-Z0-9]+)\/(\d+)$")]
    private static partial Regex WagoUrlRegex();

    private string GetAccountPath(GameVersion gameVersion) {
        var gameVersionPath = Path.Combine(gameFolder, gameVersion == GameVersion.Retail ? "_retail_" : "_classic_era");
        var accountsPath = Path.Combine(gameVersionPath, "WTF", "Account");
        var savedVariablesAccountPath = Path.Combine(accountsPath, "SavedVariables");
        var accounts = Directory.GetDirectories(accountsPath).Except([savedVariablesAccountPath]).ToList();

        if (accounts.Count > 1) {
            throw new Exception("More than one account found. Not supported yet");
        }

        return accounts.First();
    }

    public List<LocalWeakAura> GetLocalWeakAuras() {
        var accountPath = GetAccountPath(GameVersion.Retail);
        var weakAurasPath = Path.Combine(accountPath, "SavedVariables", "WeakAuras.lua");
        if (File.Exists(weakAurasPath) == false) {
            return [];
        }

        var weakAurasScript = new Script();
        weakAurasScript.DoFile(weakAurasPath);

        if (weakAurasScript.Globals["WeakAurasSaved"] is not Table weakAurasTable) {
            throw new Exception("Invalid WeakAuras structure");
        }

        if (weakAurasTable["displays"] is not Table weakAurasDisplay) {
            throw new Exception("Invalid WeakAuras structure");
        }

        var weakAuras = new List<LocalWeakAura>();

        foreach (var key in weakAurasDisplay.Keys) {
            if (weakAurasDisplay[key] is not Table weakAuraDisplay) {
                throw new Exception("Invalid WeakAuras structure");
            }

            // Skip children of each weak aura group
            if (weakAuraDisplay["parent"] != null) {
                continue;
            }

            var maybeUrl = weakAuraDisplay["url"];
            if (maybeUrl is not string url) {
                continue;
            }

            var urlMatch = WagoUrlRegex().Match(url);
            if (urlMatch.Success == false) {
                continue;
            }

            weakAuras.Add(new LocalWeakAura {
                Name = key.String,
                Slug = urlMatch.Groups[1].Value,
                Version = int.Parse(urlMatch.Groups[2].Value)
            });
        }

        return weakAuras;
    }

    private string SerializeWeakAuraUpdate(WeakAuraUpdate update) {
        var changelog = "";

        if (update.Changelog != null && update.Changelog.Text != null) {
            changelog = update.Changelog.Format == ChangelogFormat.Bbcode
                ? MarkupSanitizer.SanitizeBbCode(update.Changelog.Text)
                : MarkupSanitizer.SanitizeMarkdown(update.Changelog.Text);
        }

        List<string> lines = [
            $"[\"{update.Slug}\"] = {{",
            $"    name = [=[{update.Name}]=],",
            $"    author = [=[{update.Author}]=],",
            $"    encoded = [=[{update.Encoded}]=],",
            $"    wagoVersion = [=[{update.WagoVersion}]=],",
            $"    wagoSemver = [=[{update.WagoSemver}]=],",
            $"    source = [=[{update.Source}]=],",
            $"    versionNote = [=[{changelog}]=],",
            "}"
        ];
        return string.Join("\n", lines.ConvertAll(l => $"           {l}"));
    }

    private List<string> GenerateCompanionDataFile(List<WeakAuraUpdate> weakAuraUpdates) {
        return [
            "WowaCompanionData = {",
            "   WeakAuras = {",
            "       slugs = {",
            string.Join(",\n", weakAuraUpdates.ConvertAll(SerializeWeakAuraUpdate)),
            "       }",
            "   }",
            "}"
        ];
    }

    private void InstallCompanionAddon(GameVersion gameVersion) {
        var gameVersionPath = Path.Combine(gameFolder, gameVersion == GameVersion.Retail ? "_retail_" : "_classic_era");
        var addonPath = Path.Combine(gameVersionPath, "Interface", "AddOns", "WowaCompanion");
        if (Directory.Exists(addonPath)) {
            return;
        }

        Directory.CreateDirectory(addonPath);

        // Create the Data.lua file
        var dataLuaPath = Path.Combine(addonPath, "Data.lua");
        File.WriteAllLines(dataLuaPath, GenerateCompanionDataFile([]));

        // Create WowaCompanion.toc file
        var wowaCompanionTocPath = Path.Combine(addonPath, "WowaCompanion.toc");
        File.WriteAllLines(wowaCompanionTocPath, [
            "## Interface: 100205",
            "## Title: Wowa Companion",
            "## Author: Victor Wolmeister",
            "## Version: 1.0.0",
            "## Notes: Wowa Companion addon to keep things up to date",
            "## DefaultState: Enabled",
            "## OptionalDeps: WeakAuras",
            "",
            "Data.Lua",
            "WowaCompanion.Lua"
        ]);

        // Create WowaCompanion.toc file
        var wowaCompanionLuaPath = Path.Combine(addonPath, "WowaCompanion.lua");
        File.WriteAllLines(wowaCompanionLuaPath, [
            "local frame = CreateFrame(\"FRAME\")",
            "frame:RegisterEvent(\"ADDON_LOADED\")",
            "loadedFrame:SetScript(\"OnEvent\", function(_, _, addonName)",
            "   if addonName == \"WowaCompanion\" then",
            "       if WeakAuras and WeakAuras.AddCompanionData and WowaCompanionData then",
            "           local WeakAurasData = WowaCompanionData.WeakAuras",
            "           if WeakAurasData then",
            "                WeakAuras.AddCompanionData(WeakAurasData)",
            "           end",
            "       end",
            "   end",
            "end)"
        ]);
    }

    public async Task<List<WeakAuraUpdate>> UpdateAll() {
        var weakAuras = GetLocalWeakAuras();
        if (weakAuras.Count == 0) {
            return [];
        }

        // Install the companion addon
        InstallCompanionAddon(GameVersion.Retail);

        // Create the client
        using var client = new HttpClient();
        const string url = "https://data.wago.io/api/check/weakauras";

        // Perform the request
        var ids = weakAuras.ConvertAll(wa => $"\"{wa.Slug}\"");
        var body = $"{{\"ids\": [{string.Join(",", ids)}]}}";
        var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
        var textResponse = await response.Content.ReadAsStringAsync();

        // Parse the request
        var remoteWeakAuras = JsonSerializer.Deserialize(textResponse, typeof(List<RemoteWeakAura>),
                                  WeakAuraGenerationContext.Default) as List<RemoteWeakAura> ??
                              throw new Exception("Failed to parse response");

        List<WeakAuraUpdate> updates = [];

        foreach (var remoteWeakAura in remoteWeakAuras) {
            var installedWeakAura = weakAuras.Find(wa => wa.Slug == remoteWeakAura.Slug);
            if (installedWeakAura == null || remoteWeakAura.Version <= installedWeakAura.Version) {
                continue;
            }

            // https://data.wago.io/api/raw/encoded?id
            var encodedUrl = "https://data.wago.io/api/raw/encoded?id=" + remoteWeakAura.Slug;
            var encodedResponse = await client.GetAsync(encodedUrl);
            var encoded = await encodedResponse.Content.ReadAsStringAsync();

            updates.Add(new WeakAuraUpdate {
                Slug = remoteWeakAura.Slug,
                Name = remoteWeakAura.Name,
                Author = remoteWeakAura.Username,
                Source = "Wagp",
                WagoVersion = remoteWeakAura.Version,
                WagoSemver = remoteWeakAura.VersionString,
                Encoded = encoded,
                Changelog = remoteWeakAura.Changelog
            });
        }

        if (updates.Count <= 0) {
            return updates;
        }

        var gameVersion = GameVersion.Retail;
        var gameVersionPath =
            Path.Combine(gameFolder, gameVersion == GameVersion.Retail ? "_retail_" : "_classic_era");
        var addonPath = Path.Combine(gameVersionPath, "Interface", "AddOns", "WowaCompanion");
        var dataLuaPath = Path.Combine(addonPath, "Data.lua");
        await File.WriteAllLinesAsync(dataLuaPath, GenerateCompanionDataFile(updates));

        return updates;
    }
}