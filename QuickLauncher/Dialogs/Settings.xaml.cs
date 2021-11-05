using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Utility;
using Utility.HotKey;

namespace QuickLauncher.Dialogs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : CustomDialog
    {
        private SettingDialogModel settingDialogModel;
        public Settings(MetroWindow parent, MetroDialogSettings mySettings)
            :base(parent, mySettings)
        {
            InitializeComponent();

            settingDialogModel = new SettingDialogModel(OwningWindow);
            DataContext = settingDialogModel;
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SettingDialogModel settingDialogModel = (SettingDialogModel)DataContext;
            settingDialogModel.Save();
            await OwningWindow.HideMetroDialogAsync(this);
        }

        private void ResetOrClear_Click(object sender, RoutedEventArgs e)
        {
            settingDialogModel.ResetOrClear();
        }
    }
}
