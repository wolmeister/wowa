using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace wowa;

internal class GithubUser {
    public required string Login { get; init; }
    public required int Id { get; init; }
    public required string NodeId { get; init; }
    public required string AvatarUrl { get; init; }
    public required string GravatarId { get; init; }
    public required string Url { get; init; }
    public required string HtmlUrl { get; init; }
    public required string FollowersUrl { get; init; }
    public required string FollowingUrl { get; init; }
    public required string GistsUrl { get; init; }
    public required string StarredUrl { get; init; }
    public required string SubscriptionsUrl { get; init; }
    public required string OrganizationsUrl { get; init; }
    public required string ReposUrl { get; init; }
    public required string EventsUrl { get; init; }
    public required string ReceivedEventsUrl { get; init; }
    public required string Type { get; init; }
    public required bool SiteAdmin { get; init; }
}

internal class GithubReleaseAsset {
    public required string Url { get; init; }
    public required int Id { get; init; }
    public required string NodeId { get; init; }
    public required string Name { get; init; }
    public required string Label { get; init; }
    public required GithubUser Uploader { get; init; }
    public required string ContentType { get; init; }
    public required string State { get; init; }
    public required long Size { get; init; }
    public required int DownloadCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required string BrowserDownloadUrl { get; init; }
}

internal class GithubRelease {
    public required string Url { get; init; }
    public required string AssetsUrl { get; init; }
    public required string UploadUrl { get; init; }
    public required string HtmlUrl { get; init; }
    public required int Id { get; init; }
    public required GithubUser Author { get; init; }
    public required string NodeId { get; init; }
    public required string TagName { get; init; }
    public required string TargetCommitish { get; init; }
    public required string Name { get; init; }
    public required bool Draft { get; init; }
    public required bool Prerelease { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime PublishedAt { get; init; }
    public required List<GithubReleaseAsset> Assets { get; init; }
    public required string TarballUrl { get; init; }
    public required string ZipballUrl { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GithubRelease))]
internal partial class GithubSourceGenerationContext : JsonSerializerContext;

public class SelfUpdateManager {
    private GithubRelease GetLatestRelease() {
        // Create the client
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "wowa-app");
        const string url = "https://api.github.com/repos/wolmeister/wowa/releases/latest";

        // Perform the request
        var response = client.GetAsync(url).Result;
        var textResponse = response.Content.ReadAsStringAsync().Result;

        // Parse the request
        var jsonResponse = JsonSerializer.Deserialize(textResponse, typeof(GithubRelease),
            GithubSourceGenerationContext.Default) as GithubRelease;

        return jsonResponse ?? throw new Exception("Failed to parse response");
    }

    private Version GetLatestVersion(GithubRelease release) {
        var stringVersion = release.TagName[1..];
        return new Version(stringVersion);
    }

    private Version GetCurrentVersion() {
        var assembly = Assembly.GetEntryAssembly();
        return assembly?.GetName().Version ?? throw new Exception("Assembly information not found");
    }

    private string GetExecutablePath() {
        return Process.GetCurrentProcess().MainModule?.FileName ??
               throw new Exception("Current executable path not found");
    }

    public void Update() {
        var latestRelease = GetLatestRelease();
        var latestVersion = GetLatestVersion(latestRelease);
        var currentVersion = GetCurrentVersion();

        if (latestVersion <= currentVersion) {
            Console.WriteLine("wowa is already up to date");
            return;
        }

        Console.WriteLine("Downloading latest version...");
        using var client = new HttpClient();
        var bytes = client.GetByteArrayAsync(latestRelease.Assets[0].BrowserDownloadUrl).Result;
        var tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, bytes);
        Console.WriteLine($"Downloaded {bytes.Length} bytes, saved to {tempFile}");

        var currentFile = GetExecutablePath();
        if (File.Exists(currentFile + ".backup")) File.Delete(currentFile + ".backup");
        File.Move(currentFile, currentFile + ".backup");
        Console.WriteLine($"Moved {currentFile} to {currentFile}.backup");

        File.Move(tempFile, currentFile);
        Console.WriteLine($"Moved {tempFile} to {currentFile}");

        Console.WriteLine($"Done! Updated to {latestVersion}");
    }
}