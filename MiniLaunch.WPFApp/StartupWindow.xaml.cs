using System.Windows;

namespace MiniLaunch.WPFApp
{
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        private void Window_LocationChanged(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}