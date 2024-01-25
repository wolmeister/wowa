using System.IO.Compression;
using Kurukuru;
using wowa.Curse;

namespace wowa;

public class AddonManager(CurseApi curseApi, AddonRepository repository, string gameFolder) {
    public async Task<Addon> InstallByUrl(string url, GameVersion gameVersion) {
        // if (url.StartsWith("https://www.curseforge.com/wow/addons/") == false) throw new Exception("URL is invalid");
        var slug = url.Replace("https://www.curseforge.com/wow/addons/", "");

        var searchModsResponse = curseApi.SearchMods(new SearchModsParams {
            GameId = 1, GameVersionTypeId = gameVersion == GameVersion.Retail ? 517 : 67408, Slug = slug, Index = 0,
            SortField = SearchModsSortField.Popularity, SortOrder = SearchModsSortOrder.Desc
        });

        var curseMod = searchModsResponse.Data.Find(a => a.Slug == slug);
        if (curseMod == null) {
            throw new Exception("Addon not found");
        }

        return await Install(curseMod, gameVersion);
    }

    public void InstallById(int id, GameVersion gameVersion) {
    }

    public async Task UpdateAll() {
        // TODO - Remove the spinner creation from here.
        var addons = repository.GetAddons(null);
        var updateTasks = addons.Select(addon => Task.Run(async () => {
                await Spinner.StartAsync($"Updating addon {addon.Id}",
                    async spinner => {
                        await InstallByUrl(addon.Id, addon.GameVersion);
                        spinner.Succeed($"Updated {addon.Id} to version {addon.Version}");
                    });
            }))
            .ToList();

        await Task.WhenAll(updateTasks);
    }

    private async Task<Addon> Install(CurseMod curseMod, GameVersion gameVersion) {
        var gameVersionTypeId = gameVersion == GameVersion.Retail ? 517 : 67408;
        var fileIndex = curseMod.LatestFilesIndexes.Find(fi =>
            fi.GameVersionTypeId == gameVersionTypeId && fi.ReleaseType == CurseFileReleaseType.Release);
        if (fileIndex == null) {
            throw new Exception("File index not found");
        }

        var modFile = curseApi.GetModFile(curseMod.Id, fileIndex.FileId).Data;
        var existingAddon = repository.GetAddon(curseMod.Slug, gameVersion);
        if (existingAddon != null && existingAddon.Version == modFile.DisplayName) {
            return existingAddon;
        }

        var httpClient = new HttpClient();
        var stream = await httpClient.GetStreamAsync(modFile.DownloadUrl);

        var versionFolder = gameVersion == GameVersion.Classic ? "_classic_era_" : "_retail_";
        var addonsFolder = Path.Join(gameFolder, $"/{versionFolder}/Interface/Addons");

        if (Directory.Exists(addonsFolder) == false) {
            Directory.CreateDirectory(addonsFolder);
        }
        else {
            existingAddon?.Directories.ConvertAll(d => Path.Join(addonsFolder, d)).ForEach(d => {
                Directory.Delete(d, true);
            });
        }

        ZipFile.ExtractToDirectory(stream, addonsFolder);

        // Save the addon
        var installedAddon = new Addon {
            Id = curseMod.Slug,
            Name = curseMod.Name,
            Version = modFile.DisplayName,
            Author = curseMod.Authors.FirstOrDefault()?.Name ?? "N/A",
            GameVersion = gameVersion,
            Directories = modFile.Modules.ConvertAll(module => module.Name)
        };
        repository.SaveAddon(installedAddon);

        return installedAddon;
    }
}