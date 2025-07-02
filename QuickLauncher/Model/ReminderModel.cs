using MahApps.Metro.Controls;
using QuickLauncher.Command;
using QuickLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace QuickLauncher.Model
{

    internal class ReminderPauseItem
    {
        private string name;
        private int interval; // in minutes
        public ReminderPauseItem(String name, int interval)
        {
            this.name = name;
            this.interval = interval;
        }

        public string Name
        {
            get { return name; }
        }

        public int Interval
        {
            get { return interval; }
        }

        public DateTime ResumeTime
        {
            get { return DateTime.Now.AddMinutes(interval); }
        }
    }

    internal class ReminderModel
    {
        private Timer reminderTimer;
        private MetroWindow MainWindow;
        private SettingItem reminderInterval;
        private ObservableCollection<ReminderPauseItem> pauseItems;
        private ReminderWindow reminderWindow;

        public ReminderModel(MetroWindow mainWindow)
        {
            MainWindow = mainWindow;
#if DEBUG
            var enabledReminder = SettingItemUtils.GetEnableReminder().Value;
            bool isEnabled = enabledReminder == "1" || enabledReminder.ToLower() == "true";
            if (isEnabled)
            {
                MainWindow.BeginInvoke(() => ShowReminder());
            }
            
#endif

            pauseItems = new ObservableCollection<ReminderPauseItem>
                {
                    new ReminderPauseItem("30 minutes", 30),
                    new ReminderPauseItem("60 minutes", 60),
                    new ReminderPauseItem("2 hours", 120),
                    new ReminderPauseItem("4 hours", 120),
                    new ReminderPauseItem("6 hours", 120),
                };
        }

        public void EnabledReiminder()
        {
            var enabledReminder = SettingItemUtils.GetEnableReminder().Value;
            bool isEnabled = enabledReminder == "1" || enabledReminder.ToLower() == "true";


            if (isEnabled)
            {
                if (reminderTimer != null)
                {
                    reminderTimer.Stop();
                    reminderTimer.Dispose();
                }
                reminderInterval = SettingItemUtils.GetReminderInterval();
                reminderTimer = new Timer(Int32.Parse(reminderInterval.Value) * 60 * 1000);
                reminderTimer.Elapsed += OnTimedEvent;
                reminderTimer.Start();
                Trace.TraceInformation($"Reminder enabled with interval {reminderTimer.Interval} milseconds");

            } else
            {
                if (reminderTimer != null && reminderTimer.Enabled)
                {
                    reminderTimer.Stop();
                    reminderTimer.Dispose();
                    reminderTimer = null;
                    Trace.TraceInformation("Reminder disabled");
                }
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Trace.TraceInformation($"Reminder fired at {e.SignalTime}");
            MainWindow.BeginInvoke(() => ShowReminder());
        }

        private void ShowReminder(string interval=null)
        {
            reminderWindow = new ReminderWindow();
            reminderWindow.Interval = interval??reminderInterval.Value;
            reminderWindow.DataContext = this;
            reminderWindow.Unloaded += (s, e) =>
            {
                reminderWindow = null;
            };

            reminderWindow.Show();
        }

       public ICommand PauseReminderCommand => new SimpleCommand((x) =>
        {
            var pauseItem = pauseItems.FirstOrDefault(item => item.Name == x.ToString());
            if (pauseItem != null)
            {
                Trace.TraceInformation($"Reminder paused for {pauseItem.Interval} minutes, will resume at {pauseItem.ResumeTime}");
                if (reminderTimer != null && reminderTimer.Enabled)
                {
                    reminderTimer.Stop();
                    reminderTimer.Dispose();
                    reminderTimer = null;
                }

                if (reminderWindow != null)
                {
                    reminderWindow.Close();
                    reminderWindow = null;
                }
                var resumeTime = pauseItem.ResumeTime;
                Task.Delay(pauseItem.Interval * 60 * 1000).ContinueWith(_ =>
                {
                    MainWindow.BeginInvoke(() => ShowReminder(pauseItem.ToString()));
                    EnabledReiminder();
                    Trace.TraceInformation($"Reminder resumed at {DateTime.Now}, next reminder at {resumeTime}");
                });
            }
        });

        public ObservableCollection<ReminderPauseItem> PauseItems
        {
            get
            {
                return pauseItems;
            }
        }
    }
}
