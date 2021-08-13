using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using QuickLauncher.Dialogs;
using QuickLauncher.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Utility.Win32.Api;

namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private OpenEditorCommand OpenNewEditorCommand;
        private ObservableCollection<QuickCommand> quickCommands = new ObservableCollection<QuickCommand>();
        private QuickCommandContext dbContext = QuickCommandContext.Instance;
        public const Int32 _AboutSysMenuID = 1001;
        private SettingItem viewSetting = null;
        private MetroDialogSettings dialogSettings = new MetroDialogSettings()
        {
            ColorScheme = MetroDialogColorScheme.Accented// win.MetroDialogOptions.ColorScheme
        };
        private HashSet<string> disabledCmdContextMenuItem = new HashSet<string>(new List<string>() { "Open Working Directory", "Edit", "Edit Environment Variables", "Delete" });

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            try
            {
                loadSettings();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.StackTrace);
                DialogUtil.showError(this, "Fail to init database:" + e.Message);

                Environment.Exit(-1);
            }
            this.DataContext = this;
            OpenNewEditorCommand = new OpenEditorCommand(() =>
            {
                newquickcommand_Click(null, null);
            });

            KeyBinding OpenCmdKeyBinding = new KeyBinding(OpenNewEditorCommand, Key.N, ModifierKeys.Control);

            InputBindings.Add(OpenCmdKeyBinding);
            loadQuickCommandsFromDb("");
            commandsList.ItemsSource = quickCommands;
        }

        public SettingItem ViewMode
        {
            get
            {
                return viewSetting;
            }
        }

        private void loadSettings()
        {
            viewSetting = SettingItemUtils.GetViewMode();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /// Get the Handle for the Forms System Menu
            IntPtr systemMenuHandle = Win32Api.GetSystemMenu(this.Handle, false);

            /// Create our new System Menu items just before the Close menu item
            Win32Api.InsertMenu(systemMenuHandle, 5, Win32Api.MF_BYPOSITION | Win32Api.MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator
            Win32Api.InsertMenu(systemMenuHandle, 7, Win32Api.MF_BYPOSITION, _AboutSysMenuID, "About");

            // Attach our WndProc handler to this Window
            HwndSource source = HwndSource.FromHwnd(this.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Blue");
        }

        private void loadQuickCommandsFromDb(string key)
        {
            quickCommands.Clear();
            IQueryable<QuickCommand> query = null;
            var cleanKey = key.Trim().ToLower();
            if (cleanKey.Length == 0)
            {
                query = from b in dbContext.QuickCommands.Include("QuickCommandEnvConfigs")
                        orderby b.Alias.ToLower()
                        select b;
            }
            else
            {
                query = from b in dbContext.QuickCommands.Include("QuickCommandEnvConfigs")
                        where b.Alias.ToLower().Contains(cleanKey)
                        orderby b.Alias.ToLower()
                        select b;
            }
            try
            {
                foreach (QuickCommand qc in query)
                {
                    quickCommands.Add(qc);
                }
            }
            catch (Exception r)
            {
                Trace.TraceError(r.Message);
                Trace.TraceError(r.StackTrace);
                DialogUtil.showError(this, r.Message);
            }

        }

        private void Qle_AddedNewQuickCommand(Model.QuickCommand command)
        {
            loadQuickCommandsFromDb("");
        }

        private void keyWords_TextChanged(object sender, TextChangedEventArgs e)
        {
            var key = keyWords.Text.Trim();
            loadQuickCommandsFromDb(key);
        }

        // for details view only
        private void start_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            StartProcess(qc, false);
        }
        
        // for details view only
        private void startAdmin_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            StartProcess(qc, true);
        }


        private bool StartProcess(QuickCommand qc, bool asAdmin)
        {
            statusLabel.Content = "Starting \"" + qc.Alias + "\"";

            bool isCtrlKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            ThreadPool.QueueUserWorkItem(delegate { startProcess(qc, asAdmin); });
            if (isCtrlKeyPressed)
            {

            }
            else
            {
                this.WindowState = System.Windows.WindowState.Minimized;
            }
            
            return false;
        }

        private bool StartProcess(IList qcList, bool asAdmin)
        {
            var names = new List<string>();
            foreach (QuickCommand qc in qcList)
            {
                names.Add(qc.Alias);
            }
            var m = "Starting \"" + string.Join(", ", names) + "\", as admin = " + asAdmin;
            statusLabel.Content = m;
            Trace.TraceInformation(m);

            bool isCtrlKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            foreach (QuickCommand qc in qcList)
            {
                ThreadPool.QueueUserWorkItem(delegate { startProcess(qc, asAdmin); });
            }

            if (isCtrlKeyPressed)
            {

            }
            else
            {
                this.WindowState = System.Windows.WindowState.Minimized;
            }

            return false;
        }


        private bool startProcess(QuickCommand qc, bool asAdmin)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(qc.ExpandedPath, qc.Command);
            startInfo.UseShellExecute = true;
            if (asAdmin)
            {
                startInfo.Verb = "runas";
            }
            string workingDir = qc.ExpandedWorkDirectory;
            if (workingDir.Length == 0)
            {
                try
                {
                    workingDir = FileUtil.getParentDir(qc.ExpandedPath);
                }
                catch (UnauthorizedAccessException e)
                {
                    Debug.WriteLine(e);
                }
            }

            startInfo.WorkingDirectory = workingDir;
            if (qc.QuickCommandEnvConfigs != null)
            {
                foreach (var o in qc.QuickCommandEnvConfigs)
                {
                    startInfo.EnvironmentVariables[o.EnvKey] = o.ExpandedEnvValue;
                }
                if (qc.QuickCommandEnvConfigs.Count > 0)
                {
                    startInfo.UseShellExecute = false;
                }
            }
            try
            {
                Process.Start(startInfo);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    statusLabel.Content = "Started \"" + qc.Alias + "\" at " + DateTime.Now.ToString();
                }), DispatcherPriority.Background);

                return true;
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                Trace.TraceError(e.StackTrace);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    statusLabel.Content = "Started \"" + qc.Alias + "\" failed: \"" + e.Message + "\" at " + DateTime.Now.ToString();
                    this.WindowState = System.Windows.WindowState.Normal;
                    DialogUtil.showError(this, e.Message);
                }), DispatcherPriority.Background);
            }
            return false;
        }

        private async void edit_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;

            var dialog = new CmdEditor(this, dialogSettings, qc);
            dialog.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
            await this.ShowMetroDialogAsync(dialog);
        }

        private async void delete_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            MessageDialogResult result = await DialogUtil.ShowYesNo("Delete confirmation", this, "Are you sure to delete the selected quick commands?");
            if (result == MessageDialogResult.Affirmative)
            {
                dbContext.QuickCommandEnvConfigs.RemoveRange(qc.QuickCommandEnvConfigs);
                dbContext.QuickCommands.Remove(qc);
                dbContext.SaveChanges();
                quickCommands.Remove(qc);
            }
        }

        private void commandsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as QuickCommand;
            if (item != null)
            {
                StartProcess(item, false);
            }
        }

        /// <summary>
        /// This is the Win32 Interop Handle for this Window
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == Win32Api.WM_SYSCOMMAND)
            {
                // Execute the appropriate code for the System Menu item that was clicked
                switch (wParam.ToInt32())
                {
                    case _AboutSysMenuID:
                        handled = true;

                        About about = new About();
                        about.Owner = Application.Current.MainWindow;
                        about.ShowDialog();

                        break;
                }
            }

            return IntPtr.Zero;
        }

        private async void newquickcommand_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CmdEditor(this, dialogSettings, null);
            dialog.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
            await this.ShowMetroDialogAsync(dialog);
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Owner = this;
            about.ShowDialog();
        }

        private void startMenu_Click(object sender, RoutedEventArgs e)
        {
            StartProcess(this.commandsList.SelectedItems, false);
        }

        private void startAdminMenu_Click(object sender, RoutedEventArgs e)
        {
            StartProcess(this.commandsList.SelectedItems, true);
        }

        private void openWorkingDir_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", qc.ExpandedWorkDirectory);
            }
            catch (System.ComponentModel.Win32Exception exception)
            {
                DialogUtil.showError(this, exception.Message);
            }
        }

        private async void editEnvConfig_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            EnvEditor envEditor = new EnvEditor(this, dialogSettings, qc);
            await this.ShowMetroDialogAsync(envEditor);
        }

        private async void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings(this, dialogSettings);
            await this.ShowMetroDialogAsync(settings);
        }

        private void root_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if DEBUG
            // do nothing
#else
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
#endif
        }

        private void root_Activated(object sender, EventArgs e)
        {
            commandsList.Focus();
        }

        private void RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            loadQuickCommandsFromDb("");
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            await dbContext.Entry(qc).ReloadAsync();
        }

        private void commandsList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool enabled = commandsList.SelectedItems.Count == 1;
            ListViewItem lvi = sender as ListViewItem;
            ContextMenu cm = lvi.ContextMenu;
            foreach (object item in cm.Items)
            {
                var mi = item as MenuItem;
                if (mi == null)
                {
                    continue;
                }

                var header = (String)mi.Header;
                if (disabledCmdContextMenuItem.Contains(header))
                {
                    mi.IsEnabled = enabled;
                }
            }
        }

        private void root_Loaded(object sender, RoutedEventArgs e)
        {
            Trace.TraceInformation("start auto start command");
            IList qcList = new List<QuickCommand>();
            var query = from b in dbContext.QuickCommands.Include("QuickCommandEnvConfigs")
                        where b.IsAutoStart == true
                        select b;

            foreach (QuickCommand qc in query)
            {
                Trace.TraceInformation("auto start cmd: {0}", qc.Alias);
                qcList.Add(qc);
            }
            if (qcList.Count > 0)
            {
                StartProcess(qcList, false);
            }
            else
            {
                Trace.TraceInformation("no auto start command found");
            }
            
        }
    }
}
