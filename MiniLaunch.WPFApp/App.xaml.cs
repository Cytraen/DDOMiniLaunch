using Microsoft.Win32;
using MiniLaunch.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class App : Application
    {
        public static SoapClient SoapClient { get; set; }

        public static List<ServerInfo> ServerInfo { get; set; }

        public static Config Configuration { get; set; }

        public static Dictionary<string, string> LauncherConfig { get; set; }

        public App()
        {
            SoapClient = new SoapClient();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var startupWindow = new StartupWindow();
            startupWindow.Show();

            var updateServerInfoTask = UpdateServerInfo();
            var updateLauncherConfigTask = UpdateLauncherConfig();

            Configuration = await LoadConfig();
            SelectGameFolder(Configuration);
            await SaveConfig(Configuration);
            await CreateDatabase();

            await updateServerInfoTask;
            await updateLauncherConfigTask;

            var mainWindow = new MainWindow();
            startupWindow.Close();
            mainWindow.Show();
        }

        internal static async Task UpdateServerInfo()
        {
            var servers = await SoapClient.GetDatacenters("DDO");

            var serverList = servers.GetDatacentersResult.Datacenter.Worlds.Select(
                x => new ServerInfo
                {
                    ChatServerUrl = x.ChatServerUrl,
                    Name = x.Name,
                    Order = x.Order,
                    ServerStatusUrl = x.StatusServerUrl.Split('=').Last()
                }).ToList();

            ServerInfo = serverList.OrderBy(x => x.Order).ToList();
        }

        internal static async Task UpdateLauncherConfig()
        {
            LauncherConfig = await SoapClient.GetLauncherConfig();
        }

        internal static async Task<Config> LoadConfig()
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

            return config;
        }

        internal static Task SaveConfig(Config config)
        {
            var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            return File.WriteAllTextAsync(Config.SettingsFilePath, configJson);
        }

        private static void SelectGameFolder(Config config)
        {
            const string launcherFileName = "DNDLauncher.exe";

            while (true)
            {
                if (File.Exists(Path.Combine(config.GameDirectory, launcherFileName)))
                {
                    return;
                }

                _ = MessageBox.Show(launcherFileName + " was not found.\nPlease select your DDO installation directory.", "Can't find DDO installation directory");

                var launcherFileDialog = new OpenFileDialog
                {
                    Filter = "DDO Launcher|" + launcherFileName,
                    Title = "Select DNDLauncher.exe in your DDO install folder"
                };

                var dialogResult = launcherFileDialog.ShowDialog();

                if (!dialogResult.HasValue || !dialogResult.Value)
                {
                    _ = MessageBox.Show("DDOMiniLaunch is closing...");
                    Environment.Exit(0);
                }

                config.GameDirectory = launcherFileDialog.FileName.Replace("\\" + launcherFileName, "", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        internal static Task CreateDatabase()
        {
            if (!File.Exists(Config.DatabaseFilePath))
            {
                return Database.CreateDatabase();
            }
            return Task.CompletedTask;
        }
    }
}