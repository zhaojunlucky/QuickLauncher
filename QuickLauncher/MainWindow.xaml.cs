using ControlzEx.Theming;
using MahApps.Metro.Controls;
using QuickLauncher.Command;
using QuickLauncher.Config;
using QuickLauncher.Misc;
using QuickLauncher.Model;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Utility.HotKey;
using Utility.Win32.Api;

namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowModel viewModel;

        public const int AboutSysMenuId = 1001;

        public MainWindow()
        {
            InitializeComponent();

            Utility.Singleton.AppSingleton.Instance.StartPipeServer(ConnectionHandler);
            viewModel = new MainWindowModel(this);
            viewModel.ShowWindowNormal += ViewModel_ShowWindowNormal;
            viewModel.UpdateWindowState += ViewModel_UpdateWindowState;
            this.DataContext = viewModel;

            this.Loaded += MainWindow_Loaded;
        }

        private void ViewModel_UpdateWindowState(object sender, WindowState e)
        {
            WindowState = e;
        }

        private void ViewModel_ShowWindowNormal(object sender, object e)
        {
            ShowWindowNormal();
        }

        private void LoadQuickLaunchers()
        {

            var openEditorCommand = new OpenEditorCommand(() =>
            {
                var param = "Ctrl + N";
                if (viewModel.NewQuickCommand.CanExecute(param))
                {
                    viewModel.NewQuickCommand.Execute(param);
                }

            });

            InputBindings.Add(new KeyBinding(openEditorCommand, Key.N, ModifierKeys.Control));
        }

        // ...

        private void ConnectionHandler(IAsyncResult result)
        {
            var srv = result.AsyncState as NamedPipeServerStream;
            if (srv != null)
            {
                srv.EndWaitForConnection(result);

                // we're connected, now deserialize the incoming command line
                using (var reader = new StreamReader(srv))
                {
                    var msg = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        this.BeginInvoke(() => ShowFromProcReq(msg));
                        Utility.Singleton.AppSingleton.Instance.ReListen();
                    }
                }
            }
        }

        private void ShowFromProcReq(string message)
        {
            if (message == QlConfig.Singleton)
            {
                ShowWindowNormal();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadQuickLaunchers();

            // Get the Handle for the Forms System Menu
            IntPtr systemMenuHandle = Win32Api.GetSystemMenu(this.Handle, false);

            // Create our new System Menu items just before the Close menu item
            Win32Api.InsertMenu(systemMenuHandle, 5, Win32Api.MF_BYPOSITION | Win32Api.MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator
            Win32Api.InsertMenu(systemMenuHandle, 7, Win32Api.MF_BYPOSITION, AboutSysMenuId, "About");

            // Attach our WndProc handler to this Window
            HwndSource source = HwndSource.FromHwnd(this.Handle);
            if (source != null) source.AddHook(WndProc);

            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Blue");

            RegisterHotKeys(false);

            viewModel.Loaded();
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

        private void CommandsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is QuickCommand item)
            {
                viewModel.StartProcess(item, false);
            }
        }

        /// <summary>
        /// This is the Win32 Interop Handle for this Window
        /// </summary>
        public IntPtr Handle => new WindowInteropHelper(this).Handle;

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            // Check if a System Command has been executed
            if (msg == Win32Api.WM_SYSCOMMAND)
            {
                // Execute the appropriate code for the System Menu item that was clicked
                switch (wParam.ToInt32())
                {
                    case AboutSysMenuId:
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
            // commandsList.Focus();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            var app = (App) Application.Current;
            if (app.StartupArgs == null || app.StartupArgs.Length <= 0 || app.StartupArgs[0].Trim() != "/Restart")
            {
                viewModel.DoAutoStartCommands();
            }
        }

        private void Root_Closed(object sender, EventArgs e)
        {
            HotKeyUtil.UnRegisterHotKeys();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedTab.SelectedQuickCommands = (e.OriginalSource as ListView)?.SelectedItems.Cast<QuickCommand>()
                .ToList();
        }
    }
}
