using MiniLaunch.Common;
using System.Threading.Tasks;
using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulateAccountList().GetAwaiter().GetResult();
        }

        private async void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var newAccountWindow = new NewAccountWindow();
            newAccountWindow.ShowDialog();
            await PopulateAccountList();
        }

        private async Task PopulateAccountList()
        {
            var subs = await Database.GetSubscriptions();

            AccountListBox.Items.Clear();

            foreach (var (username, subPairs) in subs)
            {
                AccountListBox.Items.Add(username);

                foreach (var subPair in subPairs)
                {
                    AccountListBox.Items.Add("    " + subPair.Item2);
                }
            }
        }
    }
}