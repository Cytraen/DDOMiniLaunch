using AppUpdater.ClientLib;
using AppUpdater.Common;
using MiniLaunch.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class App : Application
    {
        public static ApiClient UpdateClient { get; set; }

        public static SoapClient SoapClient { get; set; }

        public static List<ServerInfo> ServerInfo { get; set; }

        public static Config Configuration { get; set; }

        public static Dictionary<string, string> LauncherConfig { get; set; }

        public App()
        {
            UpdateClient = new();
            SoapClient = new();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await UpdateApp();
            var getServerInfoTask = GetServerInfo();
            var getLauncherConfigTask = GetLauncherConfig();
            var ensureDatabaseCreatedTask = EnsureDatabaseCreated();
            var loadConfigTask = LoadConfig();

            await ensureDatabaseCreatedTask;
            Configuration = await loadConfigTask;

            if (Configuration.EnablePreview)
            {
                ServerInfo = (await getServerInfoTask).Concat(await GetServerInfo(true)).ToList();
            }
            else
            {
                ServerInfo = await getServerInfoTask;
            }

            LauncherConfig = await getLauncherConfigTask;

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        internal static async Task UpdateApp()
        {
            var info = await UpdateClient.GetProductInfo("ddoml");
            var response = info.Response;
            var currentVer = typeof(App).Assembly.GetName().Version;

            var updateAvailable = IsUpdateAvailable(currentVer, response);

            if (!updateAvailable)
            {
                return;
            }

            var result = MessageBox.Show("There is an update available.\nWould you like to update now?", "DDOMiniLaunch - Update Available", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var downloadPath = Path.Combine(Config.DataFolder, "update.zip");
            await UpdateClient.DownloadFile(response.Link, downloadPath);

            var zipFile = ZipFile.OpenRead(downloadPath);
            var dirs = zipFile.Entries.Where(x => x.Name == "" && x.Length == 0 && x.FullName.EndsWith('/')).ToList();

            if (dirs.Count == 1 && zipFile.Entries.Where(x => x.FullName.StartsWith(dirs[0].FullName)).Count() == zipFile.Entries.Count)
            {
                ZipFile.ExtractToDirectory(downloadPath, Config.DataFolder);
                Directory.Move(Path.Combine(Config.DataFolder, dirs[0].FullName), Path.Combine(Config.DataFolder, "new"));
            }
            else
            {
                ZipFile.ExtractToDirectory(downloadPath, Path.Combine(Config.DataFolder, "new"));
            }

            var files = Directory.GetFiles(Directory.GetCurrentDirectory());

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                fileInfo.MoveTo(file + ".old.updated");
            }

            var newFiles = Directory.GetFiles(Path.Combine(Config.DataFolder, "new"));

            foreach (var file in newFiles)
            {
                var fileInfo = new FileInfo(file);
                var relativePath = file.Replace(Path.Combine(Config.DataFolder, "new"), "");

                var recursiveDir = Directory.GetCurrentDirectory();

                foreach (var dir in relativePath.Split('/')[..^1])
                {
                    recursiveDir = Path.Combine(recursiveDir, dir);
                    var dirInfo = new DirectoryInfo(recursiveDir);
                    if (!dirInfo.Exists)
                    {
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), recursiveDir));
                    }
                }

                fileInfo.MoveTo(Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('/', '\\')));
            }

            //await Task.Delay(500);
            //File.Delete(Path.Combine(Config.DataFolder, "update.zip"));
            Directory.Delete(Path.Combine(Config.DataFolder, "new"), true);

            var psi = new ProcessStartInfo();

            psi.FileName = "cmd.exe";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            //psi.WorkingDirectory = Directory.GetCurrentDirectory();
            psi.Arguments = "/C timeout /T 1 /NOBREAK && powershell.exe Del " + Directory.GetCurrentDirectory() + "\\* -include *.old.updated -recurse && powershell.exe Del " + Path.Combine(Config.DataFolder, "update.zip") + " && " + Directory.GetCurrentDirectory() + "\\DDOMiniLaunch.exe";

            Process.Start(psi);
            Environment.Exit(0);
        }

        private static bool IsUpdateAvailable(Version current, ProductInfo update)
        {
            var updateVer = new Version(update.Major, update.Minor, update.Patch);

            if (updateVer > current)
            {
                return true;
            }

            return false;
        }

        private static async Task<List<ServerInfo>> GetServerInfo(bool preview = false)
        {
            var servers = await SoapClient.GetDatacenters("DDO", preview);

            return servers.GetDatacentersResult.Datacenter.Worlds.Select(
                x => new ServerInfo
                {
                    ChatServerUrl = x.ChatServerUrl,
                    Name = x.Name,
                    Order = x.Order,
                    ServerStatusUrl = x.StatusServerUrl.Split('=').Last(),
                    IsPreview = preview
                }).ToList();
        }

        private static Task<Dictionary<string, string>> GetLauncherConfig(bool preview = false)
        {
            return SoapClient.GetLauncherConfig(preview);
        }

        private static async Task<Config> LoadConfig()
        {
            var config = new Config();

            if (File.Exists(Config.SettingsFilePath))
            {
                var configText = await File.ReadAllTextAsync(Config.SettingsFilePath);

                try
                {
                    config = JsonSerializer.Deserialize<Config>(configText);
                }
                catch (JsonException)
                {
                    // ignore
                }
            }
            else
            {
                _ = Directory.CreateDirectory(Config.DataFolder);
            }

            if (!Environment.Is64BitOperatingSystem)
            {
                config.Use64Bit = false;
            }

            return config;
        }

        internal static Task SaveConfig()
        {
            var configJson = JsonSerializer.Serialize(Configuration, new JsonSerializerOptions { WriteIndented = true });
            return File.WriteAllTextAsync(Config.SettingsFilePath, configJson);
        }

        private static Task EnsureDatabaseCreated()
        {
            if (!File.Exists(Config.DatabaseFilePath))
            {
                return Database.CreateDatabase();
            }
            return Task.CompletedTask;
        }
    }
}