using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using QuickLauncher.Command;
using QuickLauncher.Dialogs;

namespace QuickLauncher.Model
{
    internal class MainWindowModel : AbstractMetroModel
    {
        private readonly QuickCommandContext dbContext = QuickCommandContext.Instance;
        private readonly MetroDialogSettings dialogSettings = new MetroDialogSettings()
        {
            ColorScheme = MetroDialogColorScheme.Accented// win.MetroDialogOptions.ColorScheme
        };
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

        public MainWindowModel(MetroWindow mainWindow)
        {
            MainWindow = mainWindow;
            QuickCommands = new ObservableCollection<QuickCommand>();
            StatusLabel = "Ready";
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
            get;
            set;
        }

        public string CommandSearchKey
        {
            get
            {
                return commandSearchKey;
            }
            set
            {
                commandSearchKey = value;
                RaisePropertyChanged("CommandSearchKey");
                LoadQuickCommands(value);
            }
        }

        public QuickCommand SelectedQuickCommand
        {
            get;
            set;
        }

        public IList<QuickCommand> SelectedQuickCommands
        {
            get;
            set;
        }

        public ICommand NewQuickCommand
        {
            get
            {
                newQuickCmd ??= new SimpleCommand(async x =>
                {
                    var dialog = new CmdEditor(MainWindow, dialogSettings, null);
                    await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog, dialogSettings);
                    LoadQuickCommands(CommandSearchKey);
                });
                return newQuickCmd;
            }
        }

        public ICommand StartCommand
        {
            get
            {
                startCmd ??= new SimpleCommand(x =>
                {
                    StartProcesses(SelectedQuickCommands as IList, false);
                });
                return startCmd;
            }
        }

        public ICommand StartAdminCommand
        {
            get
            {
                startAdminCmd ??= new SimpleCommand(x =>
                {
                    StartProcesses(SelectedQuickCommands as IList, true);
                });
                return startAdminCmd;
            }
        }

        public ICommand EditCommand
        {
            get
            {
                editCmd ??= new SimpleCommand(x =>
                {
                    var dialog = new CmdEditor(MainWindow, dialogSettings, SelectedQuickCommand);
                    _ = ShowDialogAsync(dialog, true);
                });
                return editCmd;
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                copyCmd ??= new SimpleCommand(x =>
                {
                    QuickCommand copy = new QuickCommand(SelectedQuickCommand);

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
                deleteCmd ??= new SimpleCommand(async x =>
                {
                    var qc = SelectedQuickCommand;
                    var result = await DialogUtil.ShowYesNo("Delete confirmation", this, "Are you sure to delete the selected quick commands?");
  
                    if (result == MessageDialogResult.Affirmative)
                    {
                        dbContext.QuickCommandEnvConfigs.RemoveRange(qc.QuickCommandEnvConfigs);
                        dbContext.QuickCommands.Remove(qc);
                        dbContext.SaveChanges();
                        QuickCommands.Remove(qc);
                    }
                });
                return deleteCmd;
            }
        }

        public ICommand OpenWorkingDirCommand
        {
            get
            {
                openWorkingDirCommand ??= new SimpleCommand(x =>
                {
                    try
                    {
                        Process.Start("explorer.exe", SelectedQuickCommand.ExpandedWorkDirectory);
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
                editEnvConfigCommand ??= new SimpleCommand(x =>
                {
                    EnvEditor envEditor = new EnvEditor(MainWindow, dialogSettings, SelectedQuickCommand);
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
                    _ = ShowDialogAsync(settings, false);
                });
                return settingsCommand;
            }
        }

        public ICommand RefreshAllCommand
        {
            get
            {
                refreshAllCommand ??= new SimpleCommand(x =>
                {
                    Reload();
                });
                return refreshAllCommand;
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                refreshCommand ??= new SimpleCommand(x =>
                {
                    dbContext.Entry(SelectedQuickCommand).ReloadAsync();
                });
                return refreshCommand;
            }
        }

        private async Task ShowDialogAsync(BaseMetroDialog dialog, bool reload)
        {
            await DialogCoordinator.Instance.ShowMetroDialogAsync(this, dialog, dialogSettings);
            if (reload)
            {
                LoadQuickCommands(CommandSearchKey);
            }
        }

        public void LoadQuickCommands(string key)
        {
            IQueryable<QuickCommand> query;
#nullable enable
            string? cleanKey = key?.Trim();
#nullable restore
            if (string.IsNullOrEmpty(cleanKey))
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
                QuickCommands.Clear();
                foreach (QuickCommand qc in query)
                {
                    QuickCommands.Add(qc);
                }
            }
            catch (Exception r)
            {
                Trace.TraceError(r.Message);
                Trace.TraceError(r.StackTrace);
                DialogUtil.ShowError(this, r.Message);
            }

        }

        public void Reload()
        {
            LoadQuickCommands(CommandSearchKey);
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
    }
}
