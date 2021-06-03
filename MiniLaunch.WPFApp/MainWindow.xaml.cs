using MiniLaunch.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AccountListBox.ItemsSource = Database.GetSubscriptions().GetAwaiter().GetResult();
            AccountListBox.Items.Refresh();

            ServerDropdown.ItemsSource = Enum.GetValues(typeof(Servers));
            ServerDropdown.SelectedItem = App.Configuration.LastPlayedServer;
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
            if (AccountListBox.SelectedItem is null)
            {
                AccountListBoxContextDelete.IsEnabled = false;
            }
            else
            {
                AccountListBoxContextDelete.IsEnabled = true;
            }
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            var account = (Tuple<string, string, string>)AccountListBox.SelectedItem;

            var username = account.Item1;
            var password = await Database.GetPassword(username);
            var subscriptionId = account.Item3;

            var settings = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(Config.SettingsFilePath));

            var loginResponse = await App.SoapClient.LoginAccount(username, password);
            var ticket = loginResponse.LoginAccountResult.Ticket;

            var launcherConfig = await App.SoapClient.GetLauncherConfig();
            var datacenterInfo = await App.SoapClient.GetDatacenters("DDO");

            var worldInfo = datacenterInfo.GetDatacentersResult.Datacenter.Worlds.Single(x => x.Name == ServerDropdown.Text);
            var worldStatus = await App.SoapClient.GetDatacenterStatus(worldInfo.StatusServerUrl.Split('=').Last());

            var args = launcherConfig["GameClient.WIN32.ArgTemplate"];
            args = args.Replace("{SUBSCRIPTION}", subscriptionId);
            args = args.Replace("{LOGIN}", worldStatus.loginservers.Split(';').First());
            args = args.Replace("{GLS}", ticket);
            args = args.Replace("{CHAT}", worldInfo.ChatServerUrl);
            args = args.Replace("{LANG}", "English");
            args = args.Replace("{AUTHSERVERURL}", launcherConfig["GameClient.Arg.authserverurl"]);
            args = args.Replace("{GLSTICKETLIFETIME}", launcherConfig["GameClient.Arg.glsticketlifetime"]);
            args = args.Replace("{SUPPORTURL}", launcherConfig["GameClient.Arg.supporturl"]);
            args = args.Replace("{BUGURL}", launcherConfig["GameClient.Arg.bugurl"]);
            args = args.Replace("{SUPPORTSERVICEURL}", launcherConfig["GameClient.Arg.supportserviceurl"]);

            ProcessStartInfo startInfo;

            if (App.Configuration.Use64Bit)
            {
                startInfo = new ProcessStartInfo(Path.Combine(settings.GameDirectory, "x64", "dndclient64.exe"), args);
            }
            else
            {
                startInfo = new ProcessStartInfo(Path.Combine(settings.GameDirectory, "dndclient.exe"), args);
            }

            await App.SoapClient.QueueTakeANumber(subscriptionId, ticket, worldStatus.queueurls.Split(';').First());

            startInfo.WorkingDirectory = settings.GameDirectory;

            Process.Start(startInfo);

            if (Enum.TryParse(ServerDropdown.Text, out Servers playedServer))
            {
                App.Configuration.LastPlayedServer = playedServer;
                await App.SaveConfig(settings);
            }
        }
    }
}