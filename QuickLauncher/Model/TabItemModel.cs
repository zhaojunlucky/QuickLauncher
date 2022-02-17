using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuickLauncher.Detector;

namespace QuickLauncher.Model
{
    internal abstract class TabItemModel
    {
        internal TabItemModel()
        {
            IsReadOnly = true;
            QuickCommands = new ObservableCollection<QuickCommand>();
        }
        public string Header { get; set; }
        public ObservableCollection<QuickCommand> QuickCommands { get; set; }

        public bool IsReadOnly { get; set; }

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

        internal abstract void Reload(string key);
        internal abstract void Search(string key);

        internal IList<QuickCommand> GetAutoStartCommands()
        {
            return QuickCommands.Where(qc => qc.IsAutoStart).ToList();
        }
    }

    internal sealed class UserTabItemModel : TabItemModel
    {
        private readonly QuickCommandContext dbContext = QuickCommandContext.Instance;

        public UserTabItemModel()
        {
            Reload(null);
        }

        internal override void Reload(string key)
        {
            LoadQuickCommands(key);
        }

        internal override void Search(string key)
        {
            LoadQuickCommands(key);
        }

        private void LoadQuickCommands(string key)
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
    }

    internal sealed class AutoDetectTabItemModel : TabItemModel
    {
        public AutoDetectTabItemModel(IDetector detector)
        {
            Detector = detector;
            Header = detector.Category;
            Reload(null);
        }
        public IDetector Detector { get; set; }
        internal override void Reload(string key)
        {
            Detector.Detect();
            Search(key);
        }

        internal override void Search(string key)
        {
            QuickCommands.Clear();
            foreach (var qc in Detector.QuickCommands)
            {
                if (string.IsNullOrEmpty(key))
                {
                    QuickCommands.Add(qc);
                }
                else if (qc.Alias.ToLower().Contains(key))
                {
                    QuickCommands.Add(qc);
                }
            }
        }
    }
}
