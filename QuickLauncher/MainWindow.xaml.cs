using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        public MainWindow()
        {
            InitializeComponent();
            
            this.Loaded += MainWindow_Loaded;
            try
            {
                DbUtil.prepareTables();
                loadSettings();
            }
            catch(Exception e)
            {
                DialogUtil.showError(this, "Fail to init database:" + e.Message);

                Environment.Exit(-1);
            }
            this.DataContext = this;
            OpenNewEditorCommand = new OpenEditorCommand(()=>{
                QuickLancherEditor qle = new QuickLancherEditor();
                qle.Owner = this;
                qle.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
                qle.ShowDialog();
            });
            
            KeyBinding OpenCmdKeyBinding = new KeyBinding(OpenNewEditorCommand,Key.N,ModifierKeys.Control);

            InputBindings.Add(OpenCmdKeyBinding);
            loadQuickCommandsFromDb("");
            commandsList.ItemsSource = quickCommands;
        }

        public bool IsDetailsView
        {
            get
            {
                return viewSetting.Value == "DV";
            } 
        }

        public bool IsTilesView
        {
            get
            {
                return viewSetting.Value == "TV";
            }
        }

        private void loadSettings()
        {
            viewSetting = getSettingItem("view.viewmode");
            if (viewSetting == null)
            {
                viewSetting = new SettingItem { Key = "view.viewmode", Value="TV"};
            }
            this.commandsList.Tag = viewSetting.Value;
        }

        private void saveSettingItem(SettingItem item)
        {
            try
            {
                var dbItem = getSettingItem(item.Key);
                if(dbItem == null)
                {
                    dbContext.SettingItems.Add(item);
                }
                else
                {
                    dbItem.Value = item.Value;
                }
                dbContext.SaveChanges();
            }
            catch (Exception r)
            {
                MessageBox.Show(r.InnerException.Message);
            }
        }

        private SettingItem getSettingItem(string key)
        {
            var query = from b in dbContext.SettingItems
                        where b.Key == "view.viewmode"
                        select b;
            return query.Count() == 1 ? query.First(): null;
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
        }

        private void loadQuickCommandsFromDb(string key)
        {
            quickCommands.Clear();
            IQueryable<QuickCommand> query = null;
            var cleanKey = key.Trim().ToLower();
            if(cleanKey.Length == 0)
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
            catch(Exception r)
            {
                MessageBox.Show(r.InnerException.Message);
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
            ProcessStartInfo startInfo = new ProcessStartInfo(qc.Path, qc.Command);
            if (asAdmin)
            {
                startInfo.Verb = "runas";
            }
            string workingDir = qc.WorkDirectory;
            if(workingDir.Length == 0)
            {
                try
                {
                    workingDir = FileUtil.getParentDir(qc.Path);
                }
                catch (UnauthorizedAccessException e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            
            startInfo.WorkingDirectory = workingDir; 
            if(qc.QuickCommandEnvConfigs != null)
            {
                foreach(var o in qc.QuickCommandEnvConfigs)
                {
                    startInfo.EnvironmentVariables[o.EnvKey] = o.EnvValue;
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
            catch(System.ComponentModel.Win32Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        }

        


        private void edit_Click(object sender, RoutedEventArgs e)
        {
            //QuickCommand qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommand;
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            QuickLancherEditor qle = new QuickLancherEditor();
            qle.Owner = this;
            qle.QCommand = qc;
            qle.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
            qle.ShowDialog();
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

        private void newquickcommand_Click(object sender, RoutedEventArgs e)
        {
            QuickLancherEditor qle = new QuickLancherEditor();
            qle.Owner = this;
            qle.AddedNewQuickCommand += Qle_AddedNewQuickCommand;
            qle.ShowDialog();
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
                MessageBox.Show(exception.Message);
            }
        }

        private void editEnvConfig_Click(object sender, RoutedEventArgs e)
        {
            QuickCommand qc = this.commandsList.SelectedItem as QuickCommand;
            EnvironmentVariableEditor evEditor = new EnvironmentVariableEditor(qc);
            evEditor.QuickCommandObject = qc;
            evEditor.Owner = this;
            evEditor.ShowDialog();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow win = new SettingWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ContextMenu menu = (e.Source as Control).ContextMenu;
            if (menu == null)
            {
                return;
            }
            
            foreach(var item in menu.Items)
            {
                var itemMenu = item as MenuItem;
                if(itemMenu.Header.ToString() == "View")
                {
                    var subItem = itemMenu.Items[0] as MenuItem;
                    subItem.IsChecked = IsDetailsView;
                    subItem = itemMenu.Items[1] as MenuItem;
                    subItem.IsChecked = IsTilesView;
                    break;
                }
            }
        }

        private void ChangeView_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            updateView(menuItem.Tag.ToString());
        }

        private void updateView(string view)
        {
            loadQuickCommandsFromDb("");
            viewSetting.Value = view;

            this.commandsList.Tag = viewSetting.Value;
            saveSettingItem(viewSetting);
        }

        private void root_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void root_Activated(object sender, EventArgs e)
        {
            commandsList.Focus();
        }
    }
}
