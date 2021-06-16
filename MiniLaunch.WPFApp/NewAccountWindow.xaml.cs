using MiniLaunch.Common;
using System.Linq;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class NewAccountWindow : Window
    {
        private const string AddAccountErrorTitle = "Add Account - Error";

        public NewAccountWindow()
        {
            InitializeComponent();
        }

        private async void AddNewAccount_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordTextBox.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                _ = MessageBox.Show("Must enter a username.", AddAccountErrorTitle);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                _ = MessageBox.Show("Must enter a password.", AddAccountErrorTitle);
                return;
            }

            var response = await App.SoapClient.LoginAccount(username, password, false);

            if (response.LoginAccountResult.Subscriptions.Length == 0)
            {
                _ = MessageBox.Show("No subscriptions found for account.", AddAccountErrorTitle);
                return;
            }

            var account = new Account
            {
                Username = username,
                Password = Encryption.Encrypt(password),
                Subscriptions = response.LoginAccountResult.Subscriptions.Select(x => (Subscription)x).Where(x => x != null).ToList()
            };

            await Database.AddSessionToDatabase(account);

            Close();
        }

        private void Window_LocationChanged(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}