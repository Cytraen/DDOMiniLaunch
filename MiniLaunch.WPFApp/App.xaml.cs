using Microsoft.Win32;
using MiniLaunch.Common;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class App : Application
    {
        public static SoapClient SoapClient { get; set; }

        public static Config Configuration { get; set; }

        public App()
        {
            SoapClient = new SoapClient();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var startupWindow = new StartupWindow();
            startupWindow.Show();

            Configuration = await LoadConfig();
            SelectGameFolder(Configuration);
            await SaveConfig(Configuration);

            await LoadDatabase();

            var mainWindow = new MainWindow();
            startupWindow.Close();
            mainWindow.Show();
        }

        internal static async Task<Config> LoadConfig()
        {
            var config = new Config();

            if (File.Exists(Config.SettingsFilePath))
            {
                var configText = await File.ReadAllTextAsync(Config.SettingsFilePath);
                config = JsonSerializer.Deserialize<Config>(configText);
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

                config.GameDirectory = launcherFileDialog.FileName.Replace("\\" + launcherFileName, "", System.StringComparison.InvariantCultureIgnoreCase);
            }
        }

        internal Task LoadDatabase()
        {
            if (!File.Exists(Config.DatabaseFilePath))
            {
                return Database.CreateDatabase();
            }
            return Task.CompletedTask;
        }
    }
}