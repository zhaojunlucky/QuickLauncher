using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

            var rCopyrightAttribute = (AssemblyCopyrightAttribute)rAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true)[0];
            var rCopyright = rCopyrightAttribute.Copyright;

            var info = rProduct + " " + rVersion + "\r\n\r\n" + rCopyright;
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
