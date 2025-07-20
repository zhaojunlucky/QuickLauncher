using MahApps.Metro.Controls;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using Utility.Win32.Api;
using static Utility.Win32.Api.Win32Api;

namespace QuickLauncher.Windows
{
    /// <summary>
    /// Interaction logic for Reminder.xaml
    /// </summary>
    public partial class ReminderWindow : MetroWindow
    {
        private DispatcherTimer wpfTimer;
        private int tickCount = 0;
        private const int MaxTicks = 60; // Number of ticks before the window closes

        public ReminderWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
            this.WindowStartupLocation = WindowStartupLocation.Manual; // Can set in XAML too
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            desc.Text = $"{Interval} mins: Stand up, break for health.🏃 ";
            countDown.Text = $"Closing in {MaxTicks} seconds.";
            //FlashWindow(this, FlashWindowFlags.FLASHW_ALL | FlashWindowFlags.FLASHW_TIMERNOFG);
            note.Text = SettingItemUtils.GetReminderNote().Value;

            // Get the working area of that specific screen
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;
            double screenLeft = SystemParameters.WorkArea.Left;
            double screenTop = SystemParameters.WorkArea.Top;

            // Calculate the position relative to that screen's working area
            this.Left = screenLeft + screenWidth - this.ActualWidth -2;
            this.Top = screenTop + screenHeight - this.ActualHeight;

            wpfTimer = new DispatcherTimer();
            wpfTimer.Interval = TimeSpan.FromSeconds(1); // Set the interval for the timer
            wpfTimer.Tick += WpfTimer_Tick;
            wpfTimer.Start();
        }

        private void WpfTimer_Tick(object sender, EventArgs e)
        {
            tickCount++;
            countDown.Text = $"Closing in {MaxTicks - tickCount} seconds.";
            lock (this)
            {
                if (tickCount >= MaxTicks)
                {
                    wpfTimer.Stop();
                    wpfTimer = null;
                    //FlashWindow(this, FlashWindowFlags.FLASHW_STOP);
                    this.Close(); // Close the window after MaxTicks
                }
            }
            
        }

        public string Interval
        {
            get; set;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            lock (this)
            {
                if (wpfTimer != null)
                {
                    wpfTimer.Stop();
                    wpfTimer = null;
                }
                //FlashWindow(this, FlashWindowFlags.FLASHW_STOP);
            }
        }
    }
}
