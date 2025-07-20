using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Config;
using System;
using System.Diagnostics;
using System.Windows;


namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
#if !DEBUG
        System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
#endif

        public App()
        {
            InitTraceLogger();
            if (Utility.Singleton.AppSingleton.Instance.CheckIsAppRunning(QlConfig.Singleton))
            {

                Utility.Singleton.AppSingleton.Instance.SendMsgToRunningServer(QlConfig.Singleton);
                Environment.Exit(1);
            }

            InitDb();
#if !DEBUG
            nIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            nIcon.Text = "QuickLancher By Jun";

            System.Windows.Forms.ToolStripMenuItem open = new System.Windows.Forms.ToolStripMenuItem("Open");
            open.Click += new EventHandler(NIcon_Click);
            System.Windows.Forms.ToolStripMenuItem exit = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exit.Click += new EventHandler(Exit_Click);
            System.Windows.Forms.ToolStripMenuItem about = new System.Windows.Forms.ToolStripMenuItem("About");
            about.Click += new EventHandler((o, e) =>
            {
                Show();
                About a = new About
                {
                    Owner = MainWindow
                };
                a.ShowDialog();
            });
            System.Windows.Forms.ToolStripMenuItem[] childen = new System.Windows.Forms.ToolStripMenuItem[] { about, open, exit };
            nIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            nIcon.ContextMenuStrip.Items.AddRange(childen);
            this.nIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left) Show();
            });
            nIcon.Visible = true;
#endif
        }

        public string[] StartupArgs
        {
            get;
            set;
        }
        private static void InitDb()
        {
            Trace.TraceInformation("checking db");
            DbUtil.CheckDb();
            Trace.TraceInformation("checking db end");
        }
        private static void InitTraceLogger()
        {
            var listener = new TextWriterTraceListener(QlConfig.LogFile, "quickLauncher")
            {
                TraceOutputOptions = TraceOptions.DateTime,
            };
            Trace.Listeners.Add(listener);
            Trace.AutoFlush = true;
        }

        private void NIcon_Click(object sender, EventArgs e)
        {
            //events comes here
            Show();
        }

        private async void Exit_Click(object sender, EventArgs e)
        {
            Show();
            var result = await DialogUtil.ShowYesNo("Confirm exit", (MahApps.Metro.Controls.MetroWindow)MainWindow, "Are you sure to exit?");
            if (result == MessageDialogResult.Affirmative)
            {
#if !DEBUG
                nIcon.Visible = false;
#endif
                Application.Current.Shutdown();
            }
        }

        private void Show()
        {
            ((MainWindow)MainWindow)?.ShowWindowNormal();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0 && e.Args[0].Trim() == "/Commit")
            {
                try
                {
                    Trace.TraceInformation("do database upgrade");

                    DbUtil.DoUpgradeDb();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.StackTrace);
                    MessageBox.Show("Failed to upgrade db, please delete the db or do manually upgrade. " + ex.Message);
                }

                Environment.Exit(0);
            }

            StartupArgs = e.Args;
        }

        private async void Application_ExitAsync(object sender, ExitEventArgs e)
        {
            await Utility.Singleton.AppSingleton.Instance.StopPipeServerAsync();
        }
    }
}
