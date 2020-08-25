using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Window mainWindow = null;
#if DEBUG
        private static String SINGLETON = "QuichLauncher--zj-debug";
#else
        System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        private static String SINGLETON = "QuichLauncher--zj";
#endif

        public App()
        {
            if (Utility.Singleton.AppSingleton.Instance.checkIsAppRunning(SINGLETON))
            {
                if (mainWindow == null)
                {
                    var process = Utility.Singleton.AppSingleton.getRunningInstance();
                    if (process != null)
                    {
                        Utility.Singleton.AppSingleton.sendRunningInstanceForeground(process);
                    }
                }
                else
                {
                    mainWindow.Visibility = Visibility.Visible;
                    mainWindow.WindowState = WindowState.Normal;

                    mainWindow.Show();
                    mainWindow.Activate();
                }

                Environment.Exit(1);
            }
#if !DEBUG
            nIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            
            nIcon.Text = "QuickLancher By MagicWorldZ";
            //nIcon.Click += nIcon_Click;

            System.Windows.Forms.ToolStripMenuItem open = new System.Windows.Forms.ToolStripMenuItem("Open");
            open.Click += new EventHandler(nIcon_Click);
            System.Windows.Forms.ToolStripMenuItem exit = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exit.Click += new EventHandler(exit_Click);
            System.Windows.Forms.ToolStripMenuItem about = new System.Windows.Forms.ToolStripMenuItem("About");
            about.Click += new EventHandler((o, e) =>
            {
                show();
                About a = new About();
                a.Owner = mainWindow;
                a.ShowDialog();
            });
            System.Windows.Forms.ToolStripMenuItem[] childen = new System.Windows.Forms.ToolStripMenuItem[] { about, open, exit };
            nIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            nIcon.ContextMenuStrip.Items.AddRange(childen);
            this.nIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left) show();
            });
            nIcon.Visible = true;
#endif
        }

        private void nIcon_Click(object sender, EventArgs e)
        {
            //events comes here
            show();
        }

        private void exit_Click(object sender, EventArgs e)
        {
#if !DEBUG
            nIcon.Visible = false;
#endif
            Application.Current.Shutdown();
        }

        private void show()
        {
            MainWindow.Visibility = Visibility.Visible;
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.ShowInTaskbar = true;
            MainWindow.Activate();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            mainWindow = MainWindow;
        }
    }
}
