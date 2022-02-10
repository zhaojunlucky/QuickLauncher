using MahApps.Metro.Controls;
using QuickLauncher.Model;
using System.Threading;
using System.Windows;

namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About
    {
        private readonly AboutDialogModel aboutDialogModel;
        public About()
        {
            InitializeComponent();
            aboutDialogModel = new AboutDialogModel();
            DataContext = aboutDialogModel;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string url = link.Text;
            ThreadPool.QueueUserWorkItem(delegate { System.Diagnostics.Process.Start("explorer.exe", url); });
        }

        private void DownloadNew_Click(object sender, RoutedEventArgs e)
        {
            if (aboutDialogModel.LatestRelease != null)
            {
                ThreadPool.QueueUserWorkItem(delegate { System.Diagnostics.Process.Start("explorer.exe", aboutDialogModel.LatestRelease.HtmlUrl); });
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await aboutDialogModel.CheckUpdates();
        }
    }
}
