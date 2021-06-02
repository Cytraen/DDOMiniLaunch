using Microsoft.WindowsAPICodePack.Dialogs;
using MiniLaunch.Common;
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

        private static async Task<Config> LoadConfig()
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

        private static Task SaveConfig(Config config)
        {
            var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            return File.WriteAllTextAsync(Config.SettingsFilePath, configJson);
        }

        private static void SelectGameFolder(Config config)
        {
            while (true)
            {
                if (File.Exists(Path.Combine(config.GameDirectory, "dndlauncher.exe")))
                {
                    return;
                }
                _ = MessageBox.Show("dndlauncher.exe was not found.\nPlease select your DDO installation directory.", "Can't find DDO installation directory");
                var folderPicker = new CommonOpenFileDialog("Select DDO Installation Folder") { IsFolderPicker = true };
                folderPicker.ShowDialog();
                config.GameDirectory = folderPicker.FileName;
            }
        }

        private Task LoadDatabase()
        {
            if (!File.Exists(Config.DatabaseFilePath))
            {
                return Database.CreateDatabase();
            }
            return Task.CompletedTask;
        }
    }
}