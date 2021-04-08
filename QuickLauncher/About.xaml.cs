using MahApps.Metro.Controls;
using System.Reflection;
using System.Windows;

namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : MetroWindow
    {
        public About()
        {
            InitializeComponent();

            var rAssembly = Assembly.GetEntryAssembly();
            var rProductAttribute = (AssemblyProductAttribute)rAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
            var rProduct = rProductAttribute.Product;
            var rVersion = rAssembly.GetName().Version.ToString();
            rVersion = rVersion.Substring(0, rVersion.LastIndexOf("."));

            var rCopyrightAttribute = (AssemblyCopyrightAttribute)rAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true)[0];
            var rCopyright = rCopyrightAttribute.Copyright;

            var info = "Version " + rVersion + "\r\n\r\n" + rCopyright + "\r\n";
            label.Content = info;

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(link.Text);
        }
    }
}
