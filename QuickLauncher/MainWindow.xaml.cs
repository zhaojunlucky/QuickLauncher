﻿using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Dialogs;
using QuickLauncher.Miscs;
using QuickLauncher.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Resources;
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
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            try
            {
                DbUtil.prepareTables();
                loadSettings();
            }
            catch (Exception e)
            {
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
                DialogUtil.showError(this, r.InnerException.Message);
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


        private void start_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            startProcess(qc, false);
        }
        [STAThread]
        private void startAdmin_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            startProcess(qc, true);
        }


        private void startProcess(QuickCommand qc, bool asAdmin)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(qc.ExpandedPath, qc.Command);
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
                    DialogUtil.showError(this, e.Message);
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
                System.Diagnostics.Process.Start(startInfo);
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {

                }
                else
                {
                    this.WindowState = System.Windows.WindowState.Minimized;
                }

            }
            catch (System.ComponentModel.Win32Exception e)
            {
                DialogUtil.showError(this, e.Message);
            }

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
            //QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            MessageDialogResult result = await DialogUtil.ShowYesNo("Delete confirmation", this, "Are you sure to delete this quick command?");
            if (result == MessageDialogResult.Affirmative)
            {
                var envs = dbContext.QuickCommandEnvConfigs.RemoveRange(qc.QuickCommandEnvConfigs);
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
                startProcess(item, false);
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
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            startProcess(qc, false);
        }

        private void startAdminMenu_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            startProcess(qc, true);
        }

        private void openWorkingDir_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            try
            {
                System.Diagnostics.Process.Start(qc.WorkDirectory);
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
    }
}
