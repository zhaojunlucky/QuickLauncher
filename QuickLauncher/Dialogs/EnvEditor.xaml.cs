using System;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuickLauncher.Command;

namespace QuickLauncher.Dialogs
{
    /// <summary>
    /// Interaction logic for EnvEditor.xaml
    /// </summary>
    public partial class EnvEditor
    {
        private readonly ObservableCollection<QuickCommandEnvConfig> quickCommandEnvs = new ObservableCollection<QuickCommandEnvConfig>();
        private readonly MetroWindow parent;
        private readonly QuickCommand quickCommand;
        private readonly List<QuickCommandEnvConfig> removed = new List<QuickCommandEnvConfig>();

        public EnvEditor(MetroWindow parent, MetroDialogSettings mySettings, QuickCommand quickCommand) :
            base(parent, mySettings)
        {
            InitializeComponent();
            this.quickCommand = quickCommand;
            quickCommandEnvs.Clear();
            quickCommandEnvs.CollectionChanged += QuickCommands_CollectionChanged;

            if (quickCommand.QuickCommandEnvConfigs != null)
            {
                foreach (var o in quickCommand.QuickCommandEnvConfigs)
                {
                    o.BindingEnvs = quickCommandEnvs;
                    quickCommandEnvs.Add(o);
                }
            }

            this.envGrid.DataContext = this;
            this.parent = parent;
        }

        private void QuickCommands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var o in e.NewItems)
                {
                    if (o is QuickCommandEnvConfig item)
                    {
                        item.BindingEnvs = quickCommandEnvs;
                        item.ParentId = quickCommand.Uuid;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var o in e.OldItems)
                {
                    var item = o as QuickCommandEnvConfig;
                    removed.Add(item);
                }
            }
        }

        public ObservableCollection<QuickCommandEnvConfig> EnvConfigs => quickCommandEnvs;

        public ICommand FillSysEnvVarsCmd => new SimpleCommand(async x =>
        {
            var result = await DialogUtil.ShowYesNo("Confirm", parent, "Existing variables will be kept!");
            if (result == MessageDialogResult.Affirmative)
            {
                var existingKeys = new HashSet<string>();
                foreach (var env in quickCommandEnvs)
                {
                    existingKeys.Add(env.EnvKey.ToLower());
                }

                var systemEnvs = Environment.GetEnvironmentVariables();
                foreach (var key in systemEnvs.Keys)
                {
                    if (existingKeys.Contains((key as string)?.ToLower()))
                    {
                        continue;
                    }

                    var item = new QuickCommandEnvConfig
                    {
                        BindingEnvs = quickCommandEnvs,
                        ParentId = quickCommand.Uuid,
                        EnvKey = key as string,
                        EnvValue = systemEnvs[key] as string
                    };
                    quickCommandEnvs.Add(item);

                }

                
            }
        });

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            await parent.HideMetroDialogAsync(this);
            quickCommandEnvs.CollectionChanged -= QuickCommands_CollectionChanged;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var error = HasError();
            if (error != null)
            {
                DialogUtil.ShowError(this.parent, error);
            }
            else
            {
                DoSave();
                quickCommandEnvs.CollectionChanged -= QuickCommands_CollectionChanged;
                await parent.HideMetroDialogAsync(this);
            }
        }

        private void DoSave()
        {
            var dbContext = QuickCommandContext.Instance;
            var toRemove = new List<QuickCommandEnvConfig>();
            var toAdd = new List<QuickCommandEnvConfig>();
            foreach (var r in removed)
            {
                if (!quickCommandEnvs.Contains(r))
                {
                    toRemove.Add(r);
                }
            }
            foreach (var o in quickCommandEnvs)
            {
                o.EnvKey = o.EnvKey.Trim();
                if (o.Id <= 0)
                {
                    toAdd.Add(o);
                }
            }
            dbContext.QuickCommandEnvConfigs.RemoveRange(toRemove);
            dbContext.QuickCommandEnvConfigs.AddRange(toAdd);
            dbContext.SaveChanges();

            quickCommand.QuickCommandEnvConfigs = EnvConfigs.ToList();
        }

        private string HasError()
        {
            foreach (var o in quickCommandEnvs)
            {
                if (o.Error != null)
                {
                    return o.Error;
                }
            }
            return null;
        }
    }
}
