using System.CommandLine;
using Kurukuru;
using wowa.Curse;

namespace wowa;

internal static class Program {
    private static string GetDbPath() {
        var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "wowa");
        return Path.Combine(configDir, "wowa.json");
    }

    private static void PrintAddons(List<Addon> addons) {
        List<List<string>> table = [["ID", "Name", "Version", "Game Version"]];
        var largestColumns = table[0].ConvertAll(c => c.Length);
        foreach (var addon in addons) {
            table.Add([addon.Id, addon.Name, addon.Version, addon.GameVersion.ToString().ToLower()]);
            largestColumns[0] = Math.Max(addon.Id.Length, largestColumns[0]);
            largestColumns[1] = Math.Max(addon.Name.Length, largestColumns[1]);
            largestColumns[2] = Math.Max(addon.Version.Length, largestColumns[2]);
            largestColumns[3] = Math.Max(addon.GameVersion.ToString().Length, largestColumns[3]);
        }

        foreach (var largestColumn in largestColumns) {
            for (var i = 0; i < largestColumn + 2; i++) {
                Console.Write("-");
            }
        }

        Console.Write("-\n");

        foreach (var row in table) {
            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++) {
                var column = row[columnIndex];
                Console.Write("| ");
                Console.Write(column);

                for (var i = column.Length; i < largestColumns[columnIndex]; i++) {
                    Console.Write(" ");
                }

                if (columnIndex == row.Count - 1) {
                    Console.Write("|");
                }
            }

            Console.Write("\n");
        }

        foreach (var largestColumn in largestColumns) {
            for (var i = 0; i < largestColumn + 2; i++) {
                Console.Write("-");
            }
        }

        Console.Write("-\n");
    }

    private static void Main(string[] args) {
        var dbPath = GetDbPath();
        var store = new KeyValueStore(dbPath);
        var addonRepository = new AddonRepository(store);


        // Common options
        var classicOption = new Option<bool>("--classic", "Install in the classic version of the game");
        classicOption.AddAlias("-c");
        var retailOption = new Option<bool>("--retail", "Install in the retail version of the game");
        retailOption.AddAlias("-r");
        retailOption.AddValidator(result => {
            var isClassic = result.GetValueForOption(classicOption);
            var isRetail = result.GetValueForOption(retailOption);
            if (isClassic && isRetail) {
                result.ErrorMessage = "You cannot use both --classic and --retail at the same time";
            }
        });

        // Add command
        var installCommand = new Command("install", "Install an addon");
        installCommand.AddAlias("add");
        var urlArgument = new Argument<string>("url", "Addon URL");
        installCommand.AddArgument(urlArgument);
        installCommand.AddOption(classicOption);
        installCommand.AddOption(retailOption);
        installCommand.AddValidator(result => {
            var url = result.GetValueForArgument(urlArgument);
            // if (url.StartsWith("https://www.curseforge.com/wow/addons/") == false)
            //     result.ErrorMessage = "The URl should start with 'https://www.curseforge.com/wow/addons/'";
        });
        installCommand.SetHandler(async (url, classic) => {
            var curseToken = store.Get(["config", "curse.token"]);
            var gameFolder = store.Get(["config", "game.dir"]);
            if (curseToken == null) {
                throw new Exception("Missing curse.token config");
            }

            if (gameFolder == null) {
                throw new Exception("Missing game.dir config");
            }

            var curseClient = new CurseApi(curseToken);
            var addonManager = new AddonManager(curseClient, addonRepository, gameFolder);
            Console.WriteLine("Game Folder: " + gameFolder);

            await Spinner.StartAsync($"Installing {url}...",
                async spinner => {
                    var addon = await addonManager.InstallByUrl(url,
                        classic ? GameVersion.Classic : GameVersion.Retail);
                    spinner.Succeed($"Installed {addon.Id} version {addon.Version}");
                });
        }, urlArgument, classicOption);

        // Update command
        var updateCommand = new Command("update", "Update all addons");
        updateCommand.AddAlias("up");
        updateCommand.SetHandler(async () => {
            var curseToken = store.Get(["config", "curse.token"]);
            var gameFolder = store.Get(["config", "game.dir"]);
            if (curseToken == null) {
                throw new Exception("Missing curse.token config");
            }

            if (gameFolder == null) {
                throw new Exception("Missing game.dir config");
            }

            var curseClient = new CurseApi(curseToken);
            var addonManager = new AddonManager(curseClient, addonRepository, gameFolder);
            await addonManager.UpdateAll();

            Console.WriteLine("Updating weak auras...");
            var weakAuraManager = new WeakAuraManager(gameFolder);
            var waUpdates = await weakAuraManager.UpdateAll();
            Console.WriteLine(waUpdates.Count > 0
                ? $"Updated {waUpdates.Count} weak auras"
                : "All weak auras ar up-to-date.");
        });

        // Delete command
        var deleteCommand = new Command("delete", "Delete an addon");
        deleteCommand.AddAlias("rm");
        var idArgument = new Argument<string>("id", "Addon ID");
        deleteCommand.AddArgument(idArgument);
        deleteCommand.AddOption(classicOption);
        deleteCommand.AddOption(retailOption);
        deleteCommand.SetHandler(() => throw new Exception("Not implemented yet"));

        // List command
        var listCommand = new Command("list", "List all addons");
        listCommand.AddAlias("ls");
        listCommand.AddOption(classicOption);
        listCommand.AddOption(retailOption);
        listCommand.SetHandler(() => {
            var addons = addonRepository.GetAddons(null);
            PrintAddons(addons);
        });

        // Config command
        var configCommand = new Command("config", "Manage configuration");
        var configKeyArgument = new Argument<string>("key", "Config key");
        var configValueArgument = new Argument<string>("value", "Config value");
        configValueArgument.SetDefaultValue("");
        configCommand.AddArgument(configKeyArgument);
        configCommand.AddArgument(configValueArgument);
        configCommand.SetHandler(() => throw new Exception("Not implemented yet"));

        // Self update command
        var selfUpdateCommand = new Command("self-update", "Check for new wowa updates");
        selfUpdateCommand.AddAlias("su");
        selfUpdateCommand.SetHandler(() => { new SelfUpdateManager().Update(); });

        // Root command
        var rootCommand = new RootCommand();
        rootCommand.AddCommand(installCommand);
        rootCommand.AddCommand(updateCommand);
        rootCommand.AddCommand(deleteCommand);
        rootCommand.AddCommand(listCommand);
        rootCommand.AddCommand(configCommand);
        rootCommand.AddCommand(selfUpdateCommand);
        rootCommand.Invoke(args);
    }
}