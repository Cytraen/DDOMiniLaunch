using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            var startupWindow = new StartupWindow();
            startupWindow.Show();
        }
    }
}