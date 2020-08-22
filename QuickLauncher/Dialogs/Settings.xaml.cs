using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utility;

namespace QuickLauncher.Dialogs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : CustomDialog
    {
        private SettingItem viewMode;
        public Settings(MetroWindow parent, MetroDialogSettings mySettings)
            :base(parent, mySettings)
        {
            InitializeComponent();
            viewMode = SettingItemUtils.GetViewMode();
            DataContext = this;
        }

        public bool IsAutoStart
        {
            get
            {
                try
                {
                    return AutoStartUtil.IsAutoStart();
                }
                catch (Exception r)
                {
                    DialogUtil.showError(OwningWindow, r.Message);
                    return false;
                }
            }
            set
            {
                try
                {
                    AutoStartUtil.SetAutoStart(value);
                }
                catch (Exception r)
                {
                    DialogUtil.showError(OwningWindow, r.InnerException.Message);
                }

            }
        }

        public bool ViewMode
        {
            get
            {
                return viewMode.Value == "TV";
            }
            set
            {
                viewMode.Value = value ? "TV" : "DV";
                try
                {
                    SettingItemUtils.SaveSettingItem(viewMode);
                }
                catch (Exception r)
                {
                    DialogUtil.showError(OwningWindow, r.InnerException.Message);
                }

            }
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            await OwningWindow.HideMetroDialogAsync(this);
        }
    }
}
