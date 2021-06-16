using Microsoft.Win32;
using MiniLaunch.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AboutVersionTextBlock.Text = "DDOMiniLaunch v" + typeof(App).Assembly.GetName().Version?.ToString(3);

            AccountListBox.ItemsSource = Database.GetSubscriptions().GetAwaiter().GetResult();

            if (AccountListBox.Items.Count == 1)
            {
                AccountListBox.SelectedItem = AccountListBox.Items[0];
                AccountListBoxContextDelete.IsEnabled = true;
            }

            ServerDropdown.ItemsSource = App.ServerInfo;

            var selected = App.ServerInfo.Where(x => x.Name == App.Configuration.LastPlayedServer).ToList();

            if (selected.Count == 1)
            {
                ServerDropdown.SelectedItem = selected[0];
            }
            LaunchButton.IsEnabled = LaunchButtonShouldBeEnabled();

            Use64BitCheckBox.DataContext = App.Configuration;
            EnablePreviewCheckBox.DataContext = App.Configuration;

            GameDirTextBox.DataContext = App.Configuration;
            PreviewGameDirTextBox.DataContext = App.Configuration;
        }

        private bool LaunchButtonShouldBeEnabled()
        {
            return AccountListBox.SelectedItem is not null
                && ServerDropdown.SelectedItem is not null;
        }

        private string GetGameDirectory()
        {
            var launcherFileDialog = new OpenFileDialog
            {
                Filter = "DDO Launcher|DNDLauncher.exe",
                Title = "Select DNDLauncher.exe in your DDO install folder"
            };

            var dialogResult = launcherFileDialog.ShowDialog();

            if (dialogResult != true)
            {
                return null;
            }

            return launcherFileDialog.FileName.Replace("\\DNDLauncher.exe", "", StringComparison.InvariantCultureIgnoreCase);
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var newAccountWindow = new NewAccountWindow();
            newAccountWindow.ShowDialog();
            AccountListBox.ItemsSource = Database.GetSubscriptions().GetAwaiter().GetResult();
            AccountListBox.Items.Refresh();
        }

        private async void AccountListBoxContextDelete_Click(object sender, RoutedEventArgs e)
        {
            var tuple = (Tuple<string, string, string>)AccountListBox.SelectedItem;

            await Database.DeleteSubscription(tuple.Item3, tuple.Item1);
            AccountListBox.ItemsSource = Database.GetSubscriptions().GetAwaiter().GetResult();
            AccountListBox.Items.Refresh();
        }

        private void AccountListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LaunchButton.IsEnabled = LaunchButtonShouldBeEnabled();
            AccountListBoxContextDelete.IsEnabled = AccountListBox.SelectedItem is not null;
        }

        private void ServerDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LaunchButton.IsEnabled = LaunchButtonShouldBeEnabled();
        }

        private async void Use64BitCheckBox_Click(object sender, RoutedEventArgs e)
        {
            await App.SaveConfig();
        }

        private async void EnablePreviewCheckBox_Click(object sender, RoutedEventArgs e)
        {
            await App.SaveConfig();
            MessageBox.Show("You must restart DDOMiniLaunch for this change to take effect.");
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void ChangeGameDirButton_Click(object sender, RoutedEventArgs e)
        {
            var dir = GetGameDirectory();

            if (!string.IsNullOrWhiteSpace(dir))
            {
                App.Configuration.GameDirectory = dir;
                GameDirTextBox.Text = dir;
                App.SaveConfig();
            }
        }

        private void ChangePreviewGameDirButton_Click(object sender, RoutedEventArgs e)
        {
            var dir = GetGameDirectory();

            if (!string.IsNullOrWhiteSpace(dir))
            {
                App.Configuration.PreviewGameDirectory = dir;
                PreviewGameDirTextBox.Text = dir;
                App.SaveConfig();
            }
        }

        private void SourceCodeButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/ashakoor/DDO-MiniLaunch",
                UseShellExecute = true
            });
        }

        private void DownloadsButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/ashakoor/DDO-MiniLaunch/releases",
                UseShellExecute = true
            });
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "AppUpdater.exe",
                Arguments = "ddoml DDOMiniLaunch.exe",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 2)
            {
                _ = MessageBox.Show("No update available.");
                return;
            }

            if (process.ExitCode == 3)
            {
                var result = MessageBox.Show("There is an update available.\nWould you like to update now?\nDDOMiniLaunch will close.", "DDOMiniLaunch - Update Available", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    _ = Process.Start(new ProcessStartInfo("AppUpdater.exe", output));
                    Environment.Exit(0);
                }
            }
        }

        private void CheckAssignGameDirectory(ref string directory, string defaultDir, out bool changed)
        {
            if (!string.IsNullOrWhiteSpace(directory))
            {
                var gameFound = File.Exists(Path.Combine(directory, Config.LauncherExeName));

                if (gameFound)
                {
                    changed = false;
                    return;
                }
            }

            changed = true;

            if (File.Exists(Path.Combine(defaultDir, Config.LauncherExeName)))
            {
                directory = defaultDir;
                return;
            }

            directory = GetGameDirectory();
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            var serverInfo = (ServerInfo)ServerDropdown.SelectedItem;
            var gameDir = serverInfo.IsPreview ? App.Configuration.PreviewGameDirectory : App.Configuration.GameDirectory;

            CheckAssignGameDirectory(ref gameDir,
                serverInfo.IsPreview ? Config.PreviewDefaultGameDirectory : Config.DefaultGameDirectory,
                out var changed
                );

            if (string.IsNullOrWhiteSpace(gameDir))
            {
                _ = MessageBox.Show("Must select a valid directory for DNDLauncher.exe", "DDOMiniLaunch - Invalid Directory");
                return;
            }

            if (changed)
            {
                if (serverInfo.IsPreview)
                {
                    App.Configuration.PreviewGameDirectory = gameDir;
                    PreviewGameDirTextBox.Text = gameDir;
                }
                else
                {
                    App.Configuration.GameDirectory = gameDir;
                    GameDirTextBox.Text = gameDir;
                }
            }

            var account = (Tuple<string, string, string>)AccountListBox.SelectedItem;

            var username = account.Item1;
            var subscriptionId = account.Item3;

            var encPassword = await Database.GetPassword(username);
            var password = Encryption.Decrypt(encPassword);

            var loginResponse = await App.SoapClient.LoginAccount(username, password, serverInfo.IsPreview);
            var ticket = loginResponse.LoginAccountResult.Ticket;

            var worldStatus = await App.SoapClient.GetDatacenterStatus(serverInfo.ServerStatusUrl, serverInfo.IsPreview);

            var args = App.LauncherConfig["GameClient.WIN32.ArgTemplate"]
                .Replace("{SUBSCRIPTION}", subscriptionId)
                .Replace("{LOGIN}", worldStatus.loginservers.Split(';').First())
                .Replace("{GLS}", ticket)
                .Replace("{CHAT}", serverInfo.ChatServerUrl)
                .Replace("{LANG}", "English")
                .Replace("{AUTHSERVERURL}", App.LauncherConfig["GameClient.Arg.authserverurl"])
                .Replace("{GLSTICKETLIFETIME}", App.LauncherConfig["GameClient.Arg.glsticketlifetime"])
                .Replace("{SUPPORTURL}", App.LauncherConfig["GameClient.Arg.supporturl"])
                .Replace("{BUGURL}", App.LauncherConfig["GameClient.Arg.bugurl"])
                .Replace("{SUPPORTSERVICEURL}", App.LauncherConfig["GameClient.Arg.supportserviceurl"]);

            var startInfo = new ProcessStartInfo()
            {
                Arguments = args,
                WorkingDirectory = gameDir
            };

            startInfo.FileName = App.Configuration.Use64Bit
                ? Path.Combine(gameDir, "x64", "dndclient64.exe")
                : Path.Combine(gameDir, "dndclient.exe");

            var queueUrl = worldStatus.queueurls.Split(';').First();

            var queueResult = await App.SoapClient.QueueTakeANumber(subscriptionId, ticket, queueUrl, serverInfo.IsPreview);

            if (queueResult.QueueNumberAsInt > queueResult.NowServingNumberAsInt)
            {
                while (true)
                {
                    await Task.Delay(500);
                    var currentStatus = await App.SoapClient.GetDatacenterStatus(serverInfo.ServerStatusUrl, serverInfo.IsPreview);
                    var currentQueue = currentStatus.nowservingqueuenumberAsInt;

                    if (queueResult.QueueNumberAsInt > currentQueue)
                    {
                        continue;
                    }
                    break;
                }
            }

            _ = Process.Start(startInfo);

            App.Configuration.LastPlayedServer = serverInfo.Name;
            await App.SaveConfig();
        }
    }
}