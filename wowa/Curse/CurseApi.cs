using System.Text.Json;
using System.Text.Json.Serialization;

namespace wowa.Curse;

// https://docs.curseforge.com/#tocS_Pagination
public class Pagination {
    public required int Index { get; init; }
    public required int PageSize { get; init; }
    public required int ResultCount { get; init; }
    public required long TotalCount { get; init; }
}

// https://docs.curseforge.com/#tocS_Category
public class CurseCategory {
    public required int Id { get; init; }
    public required int GameId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string Url { get; init; }
    public required string IconUrl { get; init; }
    public required string DateModified { get; init; }
    public bool? IsClass { get; init; }
    public int? ClassId { get; init; }
    public int? ParentCategoryId { get; init; }
    public int? DisplayIndex { get; init; }
}

// https://docs.curseforge.com/#tocS_ModAuthor
public class CurseModAuthor {
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
}

// https://docs.curseforge.com/#tocS_ModAsset
public class CurseModAsset {
    public required int Id { get; init; }
    public required int ModId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string ThumbnailUrl { get; init; }
    public required string Url { get; init; }
}

// https://docs.curseforge.com/#tocS_FileRelationType
public enum CurseFileReleaseType {
    Release = 1,
    Beta = 2,
    Alpha = 3
}

// https://docs.curseforge.com/#tocS_FileStatus
public enum CurseFileStatus {
    Processing = 1,
    ChangesRequired = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5,
    MalwareDetected = 6,
    Deleted = 7,
    Archived = 8,
    Testing = 9,
    Released = 10,
    ReadyForReview = 11,
    Deprecated = 12,
    Baking = 13,
    AwaitingPublishing = 14,
    FailedPublishing = 15
}

// https://docs.curseforge.com/#tocS_HashAlgo
public enum CurseHashAlgorithm {
    Sha1 = 1,
    Md5 = 2
}

// https://docs.curseforge.com/#tocS_FileHash
public class CurseFileHash {
    public required string Value { get; init; }
    public required CurseHashAlgorithm Algo { get; init; }
}

// https://docs.curseforge.com/#tocS_SortableGameVersion
public class CurseSortableGameVersion {
    public required string GameVersionName { get; init; }
    public required string GameVersionPadded { get; init; }
    public required string GameVersion { get; init; }
    public required string GameVersionReleaseDate { get; init; }
    public int? GameVersionTypeId { get; init; }
}

// https://docs.curseforge.com/#tocS_FileRelationType
public enum CurseFileRelationType {
    EmbeddedLibrary = 1,
    OptionalDependency = 2,
    RequiredDependency = 3,
    Tool = 4,
    Incompatible = 5,
    Include = 6
}

// https://docs.curseforge.com/#tocS_FileDependency
public class CurseFileDependency {
    public required int ModId { get; init; }
    public required CurseFileRelationType RelationType { get; init; }
}

// https://docs.curseforge.com/#tocS_FileModule
public class CurseFileModule {
    public required string Name { get; init; }
    public required long Fingerprint { get; init; }
}

// https://docs.curseforge.com/#tocS_File
public class CurseFile {
    public required int Id { get; init; }
    public required int GameId { get; init; }
    public required int ModId { get; init; }
    public required bool IsAvailable { get; init; }
    public required string DisplayName { get; init; }
    public required string FileName { get; init; }
    public required CurseFileReleaseType ReleaseType { get; init; }
    public required CurseFileStatus FileStatus { get; init; }
    public required List<CurseFileHash> Hashes { get; init; }
    public required string FileDate { get; init; }
    public required long FileLength { get; init; }
    public required long DownloadCount { get; init; }
    public long? FileSizeOnDisk { get; init; }
    public required string DownloadUrl { get; init; }
    public required List<string> GameVersions { get; init; }
    public required List<CurseSortableGameVersion> SortableGameVersions { get; init; }
    public required List<CurseFileDependency> Dependencies { get; init; }
    public bool? ExposeAsAlternative { get; init; }
    public int? ParentProjectFileId { get; init; }
    public int? AlternateFileId { get; init; }
    public bool? IsServerPack { get; init; }
    public int? ServerPackFileId { get; init; }
    public bool? IsEarlyAccessContent { get; init; }
    public string? EarlyAccessEndDate { get; init; }
    public required long FileFingerprint { get; init; }
    public required List<CurseFileModule> Modules { get; init; }
}

// https://docs.curseforge.com/#tocS_ModLoaderType
public enum CurseModLoaderType {
    Any = 0,
    Forge = 1,
    Cauldron = 2,
    LiteLoader = 3,
    Fabric = 4,
    Quilt = 5,
    NeoForge = 6
}

// https://docs.curseforge.com/#tocS_FileIndex
public class CurseFileIndex {
    public required string GameVersion { get; init; }
    public required int FileId { get; init; }
    public required string Filename { get; init; }
    public required CurseFileReleaseType ReleaseType { get; init; }
    public int? GameVersionTypeId { get; init; }
    public CurseModLoaderType? ModLoader { get; init; }
}

// https://docs.curseforge.com/#tocS_ModLinks
public class CurseModLinks {
    public required string WebsiteUrl { get; init; }
    public required string WikiUrl { get; init; }
    public required string IssuesUrl { get; init; }
    public required string SourceUrl { get; init; }
}

// https://docs.curseforge.com/#tocS_ModStatus
public enum CurseModStatus {
    New = 1,
    ChangesRequired = 2,
    UnderSoftReview = 3,
    Approved = 4,
    Rejected = 5,
    ChangesMade = 6,
    Inactive = 7,
    Abandoned = 8,
    Deleted = 9,
    UnderReview = 10
}

// https://docs.curseforge.com/#tocS_Mod
public class CurseMod {
    public required int Id { get; init; }
    public required int GameId { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required CurseModLinks Links { get; init; }
    public required string Summary { get; init; }
    public required CurseModStatus Status { get; init; }
    public required long DownloadCount { get; init; }
    public required bool IsFeatured { get; init; }
    public required int PrimaryCategoryId { get; init; }
    public required List<CurseCategory> Categories { get; init; }
    public int? ClassId { get; init; }
    public required List<CurseModAuthor> Authors { get; init; }
    public required CurseModAsset Logo { get; init; }
    public required List<CurseModAsset> Screenshots { get; init; }
    public required int MainFileId { get; init; }
    public required List<CurseFile> LatestFiles { get; init; }
    public required List<CurseFileIndex> LatestFilesIndexes { get; init; }
    public required List<CurseFileIndex> LatestEarlyAccessFilesIndexes { get; init; }
    public required string DateCreated { get; init; }
    public required string DateModified { get; init; }
    public required string DateReleased { get; init; }
    public bool? AllowModDistribution { get; init; }
    public required int GamePopularityRank { get; init; }
    public required bool IsAvailable { get; init; }
    public required int ThumbsUpCount { get; init; }
    public int? Rating { get; init; }
}

/** API RESPONSES / FILTERS **/
public enum SearchModsSortField {
    Featured = 1,
    Popularity = 2,
    LastUpdated = 3,
    Name = 4,
    Author = 5,
    TotalDownloads = 6,
    Category = 7,
    GameVersion = 8,
    EarlyAccess = 9,
    FeaturedReleased = 10,
    ReleasedDate = 11,
    Rating = 12
}

public enum SearchModsSortOrder {
    Asc,
    Desc
}

// TODO - This is incomplete. Implement all supported filters.
public class SearchModsParams {
    public required int GameId { get; init; }
    public int? GameVersionTypeId { get; init; }
    public string? Slug { get; init; }
    public int? Index { get; init; }
    public SearchModsSortField? SortField { get; init; }
    public SearchModsSortOrder? SortOrder { get; init; }
}

public class SearchModsResponse {
    public required List<CurseMod> Data { get; init; }
    public required Pagination Pagination { get; init; }
}

public class ModFileResponse {
    public required CurseFile Data { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SearchModsResponse))]
[JsonSerializable(typeof(ModFileResponse))]
internal partial class CurseSourceGenerationContext : JsonSerializerContext;

public class CurseApi(string token) {
    public SearchModsResponse SearchMods(SearchModsParams searchParams) {
        // Create the client
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", token);


        // Create the url
        var url = "https://api.curseforge.com/v1/mods/search?";
        url += $"gameId={searchParams.GameId}";
        if (searchParams.GameVersionTypeId != null) url += $"&gameVersionTypeId={searchParams.GameVersionTypeId}";
        if (searchParams.Slug != null) url += $"&slug={searchParams.Slug}";
        if (searchParams.Index != null) url += $"&index={searchParams.Index}";
        if (searchParams.SortField != null) url += $"&sortField={(int)searchParams.SortField}";
        if (searchParams.SortOrder != null) {
            var order = searchParams.SortOrder == SearchModsSortOrder.Asc ? "asc" : "desc";
            url += $"&sortOrder={order}";
        }

        // Perform the request
        var response = client.GetAsync(url).Result;
        var textResponse = response.Content.ReadAsStringAsync().Result;

        // Parse the request
        var jsonResponse = JsonSerializer.Deserialize(textResponse, typeof(SearchModsResponse),
            CurseSourceGenerationContext.Default) as SearchModsResponse;

        return jsonResponse ?? throw new Exception("Failed to parse response");
    }

    public ModFileResponse GetModFile(int modId, int fileId) {
        // Create the client
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-api-key", token);


        // Create the url
        var url = $"https://api.curseforge.com/v1/mods/{modId}/files/{fileId}";

        // Perform the request
        var response = client.GetAsync(url).Result;
        var textResponse = response.Content.ReadAsStringAsync().Result;

        // Parse the request
        var jsonResponse = JsonSerializer.Deserialize(textResponse, typeof(ModFileResponse),
            CurseSourceGenerationContext.Default) as ModFileResponse;

        return jsonResponse ?? throw new Exception("Failed to parse response");
    }
}