using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using IWshRuntimeLibrary;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Command;
using QuickLauncher.Detector;
using QuickLauncher.Dialogs;
using Utility;

namespace QuickLauncher.Model
{
    internal class MainWindowModel : AbstractMetroModel, IDropTarget
    {
        private readonly MetroDialogSettings dialogSettings = new MetroDialogSettings()
        {
            ColorScheme = MetroDialogColorScheme.Accented// win.MetroDialogOptions.ColorScheme
        };
        private readonly QuickCommandContext dbContext = QuickCommandContext.Instance;
        private readonly Semaphore dragDropDialogWaiter = new Semaphore(1, 1);
        public event EventHandler<WindowState> UpdateWindowState;
        public event EventHandler<object> ShowWindowNormal;
        private string commandSearchKey;
        private ICommand newQuickCmd;
        private ICommand startCmd;
        private ICommand startAdminCmd;
        private ICommand editCmd;
        private ICommand deleteCmd;
        private ICommand openWorkingDirCommand;
        private ICommand editEnvConfigCommand;
        private ICommand aboutCommand;
        private ICommand settingsCommand;
        private ICommand refreshAllCommand;
        private ICommand refreshCommand;
        private ICommand copyCmd;
        private int selectedTabIndex;
        private string statusLabel;
        private ReminderModel reminderModel;

        public MainWindowModel(MetroWindow mainWindow)
        {
            MainWindow = mainWindow;
            QuickCommands = new ObservableCollection<QuickCommand>();
            StatusLabel = "Ready";
            Tabs = new ObservableCollection<TabItemModel>
            {
                new UserTabItemModel
                {
                    Header = "Users",
                    IsReadOnly = false,
                },
            };

            if (SettingItemUtils.GetEnableAutoDetect().Value == "1")
            {
                Tabs.Add(new AutoDetectTabItemModel(new JetBrainsDetector()));
                Tabs.Add(new AutoDetectTabItemModel(new MicrosoftDetector()));
            }
            SelectedTabIndex = 0;
            reminderModel = new ReminderModel(mainWindow);

        }
        public override string this[string columnName] => throw new NotImplementedException();

        public override string Error => throw new NotImplementedException();

        public ObservableCollection<QuickCommand> QuickCommands
        {
            get;
            set;
        }

        public MetroWindow MainWindow
        {
            get;
            set;
        }

        public static string Title => "Quick Launcher";

        public string StatusLabel
        {
            get => statusLabel;
            set
            {
                statusLabel = value;
                RaisePropertyChanged("StatusLabel");
            }
        }

        public string CommandSearchKey
        {
            get => commandSearchKey;
            set
            {
                commandSearchKey = value;
                RaisePropertyChanged("CommandSearchKey");
                SearchQuickCommandsOnAllTabs(value);
            }
        }

        public ObservableCollection<TabItemModel> Tabs
        {
            get;
            set;
        }

        public int SelectedTabIndex
        {
            get => selectedTabIndex;
            set
            {
                selectedTabIndex = value;
                RaisePropertyChanged("SelectedTabIndex");
                RaisePropertyChanged("SelectedTab");
                RaisePropertyChanged("TabCommandCount");
            }
        }

        public TabItemModel SelectedTab => Tabs[SelectedTabIndex];

        public string TabCommandCount => $"{SelectedTab.QuickCommands.Count}";

        public ICommand NewQuickCommand
        {
            get
            {
                async void Execute(object x)
                {
                    var dialog = new CmdEditor(MainWindow, dialogSettings, null);
                    await ShowDialogAsync(dialog, true);
                }

                newQuickCmd ??= new SimpleCommand(x => !SelectedTab.IsReadOnly, Execute);
                return newQuickCmd;
            }
        }

        public ICommand StartCommand
        {
            get
            {
                startCmd ??= new SimpleCommand(IsQuickCommandList, x =>
                {
                    StartProcesses(x as IList, false);
                });
                return startCmd;
            }
        }

        public ICommand StartAdminCommand
        {
            get
            {
                startAdminCmd ??= new SimpleCommand(IsQuickCommandList, x =>
                {
                    StartProcesses(x as IList, true);
                });
                return startAdminCmd;
            }
        }

        public ICommand EditCommand
        {
            get
            {
                editCmd ??= new SimpleCommand(IsSingleCommand, x =>
                {
                    var dialog = new CmdEditor(MainWindow, dialogSettings, GetFirstSelectedCommand(x));
                    _ = ShowDialogAsync(dialog, true);
                });
                return editCmd;
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                copyCmd ??= new SimpleCommand(IsSingleCommandEditable, x =>
                {
                    QuickCommand copy = new QuickCommand(GetFirstSelectedCommand(x));

                    var dialog = new CmdEditor(MainWindow, dialogSettings, copy);
                    _ = ShowDialogAsync(dialog, true);
                });
                return copyCmd;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                async void Execute(object x)
                {
                    var qc = GetFirstSelectedCommand(x);
                    var result = await DialogUtil.ShowYesNo("Delete confirmation", this, "Are you sure to delete the selected quick commands?");

                    if (result == MessageDialogResult.Affirmative)
                    {
                        dbContext.QuickCommandEnvConfigs.RemoveRange(qc.QuickCommandEnvConfigs);
                        dbContext.QuickCommands.Remove(qc);
                        dbContext.SaveChanges();
                        QuickCommands.Remove(qc);
                        SelectedTab.Reload(CommandSearchKey);
                    }
                }

                deleteCmd ??= new SimpleCommand(IsSingleCommandEditable, Execute);
                return deleteCmd;
            }
        }

        public ICommand OpenWorkingDirCommand
        {
            get
            {
                openWorkingDirCommand ??= new SimpleCommand(IsSingleCommand, x =>
                {
                    try
                    {
                        Process.Start("explorer.exe", GetFirstSelectedCommand(x).ExpandedWorkDirectory);
                    }
                    catch (System.ComponentModel.Win32Exception exception)
                    {
                        DialogUtil.ShowError(this, exception.Message);
                    }
                });
                return openWorkingDirCommand;
            }
        }

        public ICommand EditEnvConfigCommand
        {
            get
            {
                editEnvConfigCommand ??= new SimpleCommand(IsSingleCommandEditable, x =>
                {
                    EnvEditor envEditor = new EnvEditor(MainWindow, dialogSettings, GetFirstSelectedCommand(x));
                    _ = ShowDialogAsync(envEditor, false);
                });
                return editEnvConfigCommand;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                aboutCommand ??= new SimpleCommand(x =>
                {
                    About about = new About
                    {
                        Owner = MainWindow
                    };
                    about.ShowDialog();
                });
                return aboutCommand;
            }
        }

        public ICommand SettingsCommand
        {
            get
            {
                settingsCommand ??= new SimpleCommand(x =>
                {
                    Settings settings = new Settings(MainWindow, dialogSettings);
                    _ = ShowDialogAsyncCallback(settings, ()=> onSettingChanged());
                });
                return settingsCommand;
            }
        }

        private void onSettingChanged()
        {
            reminderModel.EnabledReiminder();
        }

        public ICommand RefreshAllCommand
        {
            get
            {
                refreshAllCommand ??= new SimpleCommand(x =>
                {
                    SelectedTab.Reload(CommandSearchKey);
                });
                return refreshAllCommand;
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                refreshCommand ??= new SimpleCommand(IsSingleCommandEditable, x =>
                {
                    dbContext.Entry(GetFirstSelectedCommand(x)).ReloadAsync();
                });
                return refreshCommand;
            }
        }

        public ReminderModel ReminderModel
        {
            get => reminderModel;
        }

        private bool IsSingleCommandEditable(object x)
        {
            if (x == null)
            {
                return false;
            }

            IList commands = x as IList;
            if (commands == null || commands.Count == 0 || !(commands[0] is QuickCommand))
            {
                return false;
            }

            return commands.Count == 1 && !((QuickCommand) commands[0]).IsReadOnly;
        }

        private bool IsSingleCommand(object x)
        {
            if (x == null)
            {
                return false;
            }
            var commands = x as IList;
            if (commands == null || commands.Count == 0 || !(commands[0] is QuickCommand))
            {
                return false;
            }

            return commands.Count == 1;
        }

        private bool IsQuickCommandList(object x)
        {
            if (x == null)
            {
                return false;
            }

            return x is IList commands && commands.Count > 0 && commands[0] is QuickCommand;
        }

        private static QuickCommand GetFirstSelectedCommand(object x)
        {
            if (x is IList commands) return commands[0] as QuickCommand;
            return null;
        }

        private async Task ShowDialogAsync(BaseMetroDialog dialog, bool reload)
        {
            if (reload)
            {
                dialog.Unloaded += (sender, args) =>
                {
                    SelectedTab.Reload(CommandSearchKey);
                };
            }
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog, dialogSettings);
        }

        private async Task ShowDialogAsyncCallback(BaseMetroDialog dialog, Action callback)
        {
            if (callback != null)
            {
                dialog.Unloaded += (sender, args) =>
                {
                    callback();
                };
            }
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog, dialogSettings);
        }

        private void SearchQuickCommandsOnAllTabs(string value)
        {
            foreach (var tab in Tabs)
            {
                MainWindow.Dispatcher.InvokeAsync(() => tab.Search(value));
            }
        }

        public bool StartProcess(QuickCommand qc, bool asAdmin)
        {
            StatusLabel = $"Starting '{qc.Alias}'";

            bool isCtrlKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            ThreadPool.QueueUserWorkItem(delegate { StartProcessInternal(qc, asAdmin); });
            if (!isCtrlKeyPressed)
            {
                UpdateWindowState?.Invoke(this, WindowState.Minimized);
            }

            return false;
        }

        public bool StartProcesses(IList qcList, bool asAdmin)
        {
            var names = new List<string>();
            foreach (QuickCommand qc in qcList)
            {
                names.Add(qc.Alias);
            }
            var m = $"Starting '{string.Join(", ", names)}', as admin = {asAdmin}";
            StatusLabel = m;
            Trace.TraceInformation(m);

            bool isCtrlKeyPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            foreach (QuickCommand qc in qcList)
            {
                ThreadPool.QueueUserWorkItem(delegate { StartProcessInternal(qc, asAdmin); });
            }

            if (!isCtrlKeyPressed)
            {
                UpdateWindowState?.Invoke(this, WindowState.Minimized);
            }

            return false;
        }


        private bool StartProcessInternal(QuickCommand qc, bool asAdmin)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(qc.ExpandedPath, qc.Command)
            {
                UseShellExecute = qc.UseShellExecute,
                CreateNoWindow = qc.CreateNoWindow,
            };
            Trace.TraceInformation("Start process path {0} cmd {1} admin {2}, use shell {3} no window {4}",
                qc.ExpandedPath, qc.Command, asAdmin, qc.UseShellExecute, qc.CreateNoWindow);
            if (asAdmin)
            {
                startInfo.Verb = "runas";
            }
            string workingDir = qc.ExpandedWorkDirectory;
            if (workingDir.Length == 0)
            {
                workingDir = FileUtil.GetDirectoryOfFile(qc.ExpandedPath);
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
                    Trace.TraceInformation("force use shell execute false for env config");
                }
            }
            try
            {
                Process.Start(startInfo);
                StatusLabel = $"Started '{qc.Alias}' at {DateTime.Now}";

                return true;
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                Trace.TraceError(e.StackTrace);
                if (e.NativeErrorCode != 1223) // if it is not canceled by the user
                                               // see https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
                {
                    StatusLabel = $"Started '{qc.Alias}' failed: {e.Message} at {DateTime.Now}";
                    MainWindow.BeginInvoke(() =>
                    {
                        ShowWindowNormal?.Invoke(this, this);
                        DialogUtil.ShowError(this, e.Message);
                    });
                    
                }

            }
            return false;
        }

        internal void DoAutoStartCommands()
        {
           
            var qcList = new List<QuickCommand>();

            foreach (var tab in Tabs)
            {
                qcList.AddRange(tab.GetAutoStartCommands());
            }

            Trace.TraceInformation("start auto start command");

            if (qcList.Count > 0)
            {
                StartProcesses(qcList, false);
            }

        }

        private bool IsSupportedFile(string filePath)
        {
            var fileLower = filePath.ToLower();
            return fileLower.EndsWith(".lnk") || fileLower.EndsWith(".exe");
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragDropEffects effect = DragDropEffects.None;
            if (dropInfo.Data is DataObject data)
            {
                foreach(var file in data.GetFileDropList())
                {
                    if (!IsSupportedFile(file)) continue;
                    effect = DragDropEffects.Link;
                    break;
                }
            }

            dropInfo.Effects = effect;

        }

        public void Drop(IDropInfo dropInfo)
        {
            var fileList = new List<string>();

            if (!(dropInfo.Data is DataObject data)) return;

            foreach (var file in data.GetFileDropList())
            {
                if (!IsSupportedFile(file)) continue;

                fileList.Add(file);
            }
            Trace.TraceInformation("dropped file list {0}", fileList.ToString());
            ThreadPool.QueueUserWorkItem(delegate { CreateCommandsForDrop(fileList); });
            
        }

        private void CreateCommandsForDrop(List<string> fileList)
        {
            foreach (var file in fileList)
            {
                dragDropDialogWaiter.WaitOne();
                MainWindow.BeginInvoke(()=> CreateCommandForDrop(file));
            }
        }

        private async void CreateCommandForDrop(string file)
        {
            IWshShell wsh = new WshShell();
            var qc = new QuickCommand(true);
            if (file.ToLower().EndsWith(".lnk"))
            {
                IWshShortcut sc = (IWshShortcut)wsh.CreateShortcut(file);
                var workingDir = sc.WorkingDirectory;
                if (string.IsNullOrEmpty(workingDir))
                {
                    qc.WorkDirectory = Path.GetDirectoryName(sc.TargetPath);
                }

                qc.Path = sc.TargetPath;
                qc.Command = sc.Arguments;
                qc.Alias = FileUtil.GetFileNameNoExt(file);
            }
            else
            {
                qc.Path = file;
            }
            var dialog = new CmdEditor(MainWindow, dialogSettings, qc);

            dialog.BeforeClose += delegate(object sender)
            {
                if (sender != dialog) return;
                Trace.TraceInformation("{0} dialog closed", file);
                dragDropDialogWaiter.Release();

            };
            Trace.TraceInformation("handle drop {0}", file);

            await ShowDialogAsync(dialog, true);
        }

        internal void Loaded()
        {
            reminderModel.EnabledReiminder();
        }
    }
}
