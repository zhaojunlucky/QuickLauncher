using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        private static Window mainWindow = null;

#if DEBUG
        private static String SINGLETON = "QuichLauncher--zj-debug";
#else
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

            nIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            
            nIcon.Text = "QuickLancher By MagicWorldZ";
            nIcon.Click += nIcon_Click;

            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("Open");
            open.Click += new EventHandler(nIcon_Click);
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit");
            exit.Click += new EventHandler(exit_Click);
            System.Windows.Forms.MenuItem about = new System.Windows.Forms.MenuItem("About");
            about.Click += new EventHandler((o,e)=>
            {
                About a = new About();
                a.ShowDialog();
            });
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { about, open, exit};
            nIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            this.nIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left) show();
            });
            nIcon.Visible = true;
        }

        private void nIcon_Click(object sender, EventArgs e)
        {
            //events comes here
            show();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            nIcon.Visible = false;
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
