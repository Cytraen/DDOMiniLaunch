using CSharpDate.ClientLib;
using MiniLaunch.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class App : Application
    {
        public static CSharpDateClient UpdateClient { get; set; }

        public static SoapClient SoapClient { get; set; }

        public static List<ServerInfo> ServerInfo { get; set; }

        public static Config Configuration { get; set; }

        public static Dictionary<string, string> LauncherConfig { get; set; }

        public App()
        {
            UpdateClient = new("ddoml", "https://update.cytn.xyz");

            UpdateClient.ExecutableName = "DDOMiniLaunch.exe";
            UpdateClient.NoUpdateMessage = () => MessageBox.Show("No updates available.", "DDOMiniLaunch");
            UpdateClient.UpdateConfirmation = () => MessageBox.Show("There is an update available.\nWould you like to update now?", "DDOMiniLaunch - Update Available", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

            SoapClient = new();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Configuration = await LoadConfig();

            if (Configuration.CheckUpdateAtStartup)
            {
                try
                {
                    await UpdateApp(true);
                }
                catch (Exception ex) when (ex is HttpRequestException)
                {
                    // ignored
                }
            }

            var getServerInfoTask = GetServerInfo();
            var getLauncherConfigTask = GetLauncherConfig();
            var ensureDatabaseCreatedTask = EnsureDatabaseCreated();

            await ensureDatabaseCreatedTask;

            ServerInfo = Configuration.EnablePreview ? (await getServerInfoTask).Concat(await GetServerInfo(true)).ToList() : await getServerInfoTask;
            LauncherConfig = await getLauncherConfigTask;

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        internal static Task UpdateApp(bool shouldWarn)
        {
            try
            {
                return UpdateClient.CheckAndDownloadUpdate(typeof(App).Assembly.GetName().Version, shouldWarn);
            }
            catch (Exception ex) when (ex is HttpRequestException)
            {
                throw;
            }
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