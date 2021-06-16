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

        public static SoapClient SoapPreviewClient { get; set; }

        public static List<ServerInfo> ServerInfo { get; set; }

        public static Config Configuration { get; set; }

        public static Dictionary<string, string> LauncherConfig { get; set; }

        public App()
        {
            SoapClient = new SoapClient();
            SoapPreviewClient = new SoapClient(true);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var startupWindow = new StartupWindow();
            startupWindow.Show();

            var updateServerInfoTask = UpdateServerInfo();
            var updateLauncherConfigTask = UpdateLauncherConfig();

            await LoadConfig();

            SelectGameDirectory(Configuration, out var isDirChanged);

            if (isDirChanged)
            {
                await SaveConfig();
            }

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
                    ServerStatusUrl = x.StatusServerUrl.Split('=').Last(),
                    IsPreview = false
                }).ToList();

            if (Configuration.EnablePreview)
            {
                var previewServers = await SoapPreviewClient.GetDatacenters("DDO");

                var previewServerList = previewServers.GetDatacentersResult.Datacenter.Worlds.Select(
                    x => new ServerInfo
                    {
                        ChatServerUrl = x.ChatServerUrl,
                        Name = x.Name,
                        Order = x.Order,
                        ServerStatusUrl = x.StatusServerUrl.Split('=').Last(),
                        IsPreview = true
                    }).ToList();

                ServerInfo = serverList.Concat(previewServerList).OrderBy(x => x.IsPreview).ThenBy(x => x.Order).ToList();
            }
            else
            {
                ServerInfo = serverList.OrderBy(x => x.Order).ToList();
            }
        }

        internal static async Task UpdateLauncherConfig()
        {
            LauncherConfig = await SoapClient.GetLauncherConfig();
        }

        internal static async Task LoadConfig()
        {
            Configuration = new Config();

            if (File.Exists(Config.SettingsFilePath))
            {
                var configText = await File.ReadAllTextAsync(Config.SettingsFilePath);

                try
                {
                    Configuration = JsonSerializer.Deserialize<Config>(configText);
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

            if (Configuration.Use64Bit && !Environment.Is64BitOperatingSystem)
            {
                Configuration.Use64Bit = false;
            }
        }

        internal static Task SaveConfig()
        {
            var configJson = JsonSerializer.Serialize(Configuration, new JsonSerializerOptions { WriteIndented = true });
            return File.WriteAllTextAsync(Config.SettingsFilePath, configJson);
        }

        private static void SelectGameDirectory(Config config, out bool isChanged, bool preview = false)
        {
            isChanged = false;
            const string launcherFileName = "DNDLauncher.exe";

            while (true)
            {
                if (File.Exists(Path.Combine(config.GameDirectory, launcherFileName)))
                {
                    return;
                }

                isChanged = true;
                _ = MessageBox.Show(launcherFileName + " was not found.\nPlease select your DDO installation directory.", "Can't find DDO installation directory");

                var launcherFileDialog = new OpenFileDialog
                {
                    Filter = "DDO Launcher|" + launcherFileName,
                    Title = "Select " + launcherFileName + " in your DDO install folder"
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