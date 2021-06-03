using MiniLaunch.Common;
using System;
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

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}