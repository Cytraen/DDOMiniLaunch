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

            AccountListBox.ItemsSource = Database.GetSubscriptions().GetAwaiter().GetResult();
            AccountListBox.Items.Refresh();

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
            EnableLaunchButton();

            Use64BitCheckBox.DataContext = App.Configuration;
            Use64BitCheckBox.IsEnabled = Environment.Is64BitOperatingSystem;
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

        private void EnableLaunchButton()
        {
            LaunchButton.IsEnabled = AccountListBox.SelectedItem is not null && ServerDropdown.SelectedItem is not null;
        }

        private void AccountListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            EnableLaunchButton();
            AccountListBoxContextDelete.IsEnabled = AccountListBox.SelectedItem is not null;
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            var account = (Tuple<string, string, string>)AccountListBox.SelectedItem;

            var username = account.Item1;
            var encPassword = await Database.GetPassword(username);

            var password = Encryption.Decrypt(encPassword);

            var subscriptionId = account.Item3;

            var loginResponse = await App.SoapClient.LoginAccount(username, password);
            var ticket = loginResponse.LoginAccountResult.Ticket;

            var worldInfo = (ServerInfo)ServerDropdown.SelectedItem;
            var worldStatus = await App.SoapClient.GetDatacenterStatus(worldInfo.ServerStatusUrl);

            var args = App.LauncherConfig["GameClient.WIN32.ArgTemplate"];
            args = args.Replace("{SUBSCRIPTION}", subscriptionId);
            args = args.Replace("{LOGIN}", worldStatus.loginservers.Split(';').First());
            args = args.Replace("{GLS}", ticket);
            args = args.Replace("{CHAT}", worldInfo.ChatServerUrl);
            args = args.Replace("{LANG}", "English");
            args = args.Replace("{AUTHSERVERURL}", App.LauncherConfig["GameClient.Arg.authserverurl"]);
            args = args.Replace("{GLSTICKETLIFETIME}", App.LauncherConfig["GameClient.Arg.glsticketlifetime"]);
            args = args.Replace("{SUPPORTURL}", App.LauncherConfig["GameClient.Arg.supporturl"]);
            args = args.Replace("{BUGURL}", App.LauncherConfig["GameClient.Arg.bugurl"]);
            args = args.Replace("{SUPPORTSERVICEURL}", App.LauncherConfig["GameClient.Arg.supportserviceurl"]);

            var startInfo = new ProcessStartInfo()
            {
                Arguments = args,
                WorkingDirectory = App.Configuration.GameDirectory
            };

            startInfo.FileName = App.Configuration.Use64Bit
                ? Path.Combine(App.Configuration.GameDirectory, "x64", "dndclient64.exe")
                : Path.Combine(App.Configuration.GameDirectory, "dndclient.exe");

            var queueUrl = worldStatus.queueurls.Split(';').First();

            var queueResult = await App.SoapClient.QueueTakeANumber(subscriptionId, ticket, queueUrl);

            if (queueResult.QueueNumberAsInt > queueResult.NowServingNumberAsInt)
            {
                while (true)
                {
                    await Task.Delay(500);
                    var currentStatus = await App.SoapClient.GetDatacenterStatus(worldInfo.ServerStatusUrl);
                    var currentQueue = currentStatus.nowservingqueuenumberAsInt;

                    if (queueResult.QueueNumberAsInt > currentQueue)
                    {
                        continue;
                    }
                    break;
                }
            }

            _ = Process.Start(startInfo);

            App.Configuration.LastPlayedServer = ((ServerInfo)ServerDropdown.SelectedItem).Name;
            await App.SaveConfig();
        }

        private void ServerDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            EnableLaunchButton();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private async void Use64BitCheckBox_Click(object sender, RoutedEventArgs e)
        {
            await App.SaveConfig();
        }
    }
}