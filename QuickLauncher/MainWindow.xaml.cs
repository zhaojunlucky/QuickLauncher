using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using QuickLauncher.Config;
using QuickLauncher.Dialogs;
using QuickLauncher.Misc;
using QuickLauncher.Model;
using QuickLauncher.Notification;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Utility;
using Utility.HotKey;
using Utility.Win32.Api;

namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private OpenEditorCommand OpenNewEditorCommand;
        private readonly ObservableCollection<QuickCommand> quickCommands = new ObservableCollection<QuickCommand>();
        private readonly QuickCommandContext dbContext = QuickCommandContext.Instance;
        public const Int32 _AboutSysMenuID = 1001;
        private readonly MetroDialogSettings dialogSettings = new MetroDialogSettings()
        {
            ColorScheme = MetroDialogColorScheme.Accented// win.MetroDialogOptions.ColorScheme
        };
        private readonly HashSet<string> disabledCmdContextMenuItem = new HashSet<string>(new List<string>() { "Open Working Directory", "Edit", "Edit Environment Variables", "Delete", "Copy" });

        public MainWindow()
        {
            InitializeComponent();

            Utility.Singleton.AppSingleton.Instance.StartPipeServer(new AsyncCallback(ConnectionHandler));

            this.Loaded += MainWindow_Loaded;
        }

        private void LoadQuickLaunchers()
        {
            Trace.TraceInformation("loading from database");
 
            LoadQuickCommandsFromDb("");
            Trace.TraceInformation("loading from database - done");

            this.DataContext = this;
            OpenNewEditorCommand = new OpenEditorCommand(() =>
            {
                Newquickcommand_Click(null, null);
            });

            KeyBinding OpenCmdKeyBinding = new KeyBinding(OpenNewEditorCommand, Key.N, ModifierKeys.Control);

            InputBindings.Add(OpenCmdKeyBinding);

            commandsList.ItemsSource = quickCommands;
        }

        public ObservableCollection<QuickCommand> QuickCommands
        {
            get
            {
                return quickCommands;
            }
        }

        private void ConnectionHandler(IAsyncResult result)
        {
            var srv = result.AsyncState as NamedPipeServerStream;
            srv.EndWaitForConnection(result);

            // we're connected, now deserialize the incoming command line
            var bf = new BinaryFormatter();
            var msg = bf.Deserialize(srv) as string;

            if (msg != "")
            {
                this.BeginInvoke((Action)(() => ShowFromProcReq(msg)));
                Utility.Singleton.AppSingleton.Instance.ReListen();
            }
        }

        private void ShowFromProcReq(string messgae)
        {
            if (messgae == QLConfig.Singleton)
            {
                ShowWindowNormal();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadQuickLaunchers();

            /// Get the Handle for the Forms System Menu
            IntPtr systemMenuHandle = Win32Api.GetSystemMenu(this.Handle, false);

            /// Create our new System Menu items just before the Close menu item
            Win32Api.InsertMenu(systemMenuHandle, 5, Win32Api.MF_BYPOSITION | Win32Api.MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator
            Win32Api.InsertMenu(systemMenuHandle, 7, Win32Api.MF_BYPOSITION, _AboutSysMenuID, "About");

            // Attach our WndProc handler to this Window
            HwndSource source = HwndSource.FromHwnd(this.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Blue");

            RegisterHotKeys(false);
        }

        public void RegisterHotKeys(bool reload)
        {
            if (reload)
            {
                HotKeyUtil.UnRegisterHotKeys();
            }
            HotKeyUtil.RegisterHotKey(SettingItemUtils.GetMainWindowOpenHotkey(), OnOpenWindowHotKey);
        }

        private void OnOpenWindowHotKey(object sender, HotkeyEventArgs e)
        {
            ShowWindowNormal();
        }

        public void ShowWindowNormal()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            if (Visibility != Visibility.Visible)
            {
                Visibility = Visibility.Visible;
            }

            Activate();
        }

        private void LoadQuickCommandsFromDb(string key)
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
            LoadQuickCommandsFromDb("");
        }

        private void KeyWords_TextChanged(object sender, TextChangedEventArgs e)
        {
            var key = keyWords.Text.Trim();
            LoadQuickCommandsFromDb(key);
        }

        // for details view only
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            StartProcess(qc, false);
        }
        
        // for details view only
        private void StartAdmin_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            StartProcess(qc, true);
        }


        private bool StartProcess(QuickCommand qc, bool asAdmin)
        {
            statusLabel.Content = "Starting \"" + qc.Alias + "\"";

            bool isCtrlKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            ThreadPool.QueueUserWorkItem(delegate { StartProcessInternal(qc, asAdmin); });
            if (!isCtrlKeyPressed)
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
                ThreadPool.QueueUserWorkItem(delegate { StartProcessInternal(qc, asAdmin); });
            }

            if (!isCtrlKeyPressed)
            {
                this.WindowState = System.Windows.WindowState.Minimized;
            }

            return false;
        }


        private bool StartProcessInternal(QuickCommand qc, bool asAdmin)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(qc.ExpandedPath, qc.Command)
            {
                UseShellExecute = true
            };
            if (asAdmin)
            {
                startInfo.Verb = "runas";
            }
            string workingDir = qc.ExpandedWorkDirectory;
            if (workingDir.Length == 0)
            {
                workingDir = FileUtil.getDirectoryOfFile(qc.ExpandedPath);
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
                if(e.NativeErrorCode != 1223) // if it is not canceled by the user
                                              // see https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        statusLabel.Content = "Started \"" + qc.Alias + "\" failed: \"" + e.Message + "\" at " + DateTime.Now.ToString();
                        ShowWindowNormal();
                        DialogUtil.showError(this, e.Message);
                    }), DispatcherPriority.Background);
                }
                
            }
            return false;
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;

            var dialog = new CmdEditor(this, dialogSettings, qc);
            dialog.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
            await this.ShowMetroDialogAsync(dialog);
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
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

        private void CommandsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is QuickCommand item)
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

                        About about = new About
                        {
                            Owner = Application.Current.MainWindow
                        };
                        about.ShowDialog();

                        break;
                }
            }

            return IntPtr.Zero;
        }

        private async void Newquickcommand_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CmdEditor(this, dialogSettings, null);
            dialog.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
            await this.ShowMetroDialogAsync(dialog);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = this
            };
            about.ShowDialog();
        }

        private void StartMenu_Click(object sender, RoutedEventArgs e)
        {
            StartProcess(this.commandsList.SelectedItems, false);
        }

        private void StartAdminMenu_Click(object sender, RoutedEventArgs e)
        {
            StartProcess(this.commandsList.SelectedItems, true);
        }

        private void OpenWorkingDir_Click(object sender, RoutedEventArgs e)
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

        private async void EditEnvConfig_Click(object sender, RoutedEventArgs e)
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

        private void Root_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if DEBUG
            // do nothing
#else
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
#endif
        }

        private void Root_Activated(object sender, EventArgs e)
        {
            commandsList.Focus();
        }

        private void RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            LoadQuickCommandsFromDb("");
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            await dbContext.Entry(qc).ReloadAsync();
        }

        private void CommandsList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool enabled = commandsList.SelectedItems.Count == 1;
            ListViewItem lvi = sender as ListViewItem;
            ContextMenu cm = lvi.ContextMenu;
            foreach (object item in cm.Items)
            {
                if (item is MenuItem mi)
                {
                    var header = (String)mi.Header;
                    if (disabledCmdContextMenuItem.Contains(header))
                    {
                        mi.IsEnabled = enabled;
                    }
                }
                
            }
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
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
        }

        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            QuickCommand copy = new QuickCommand(qc);

            var dialog = new CmdEditor(this, dialogSettings, copy);
            dialog.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
            await this.ShowMetroDialogAsync(dialog);
        }

        private void Root_Closed(object sender, EventArgs e)
        {
            HotKeyUtil.UnRegisterHotKeys();
        }
    }
}
