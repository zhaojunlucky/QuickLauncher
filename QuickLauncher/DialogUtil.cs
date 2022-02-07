using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;

namespace QuickLauncher
{
    public class DialogUtil
    {
        public static void ShowWarning(MetroWindow win, string message)
        {
            SimpleMessageBox("Warning", win, message);
        }

        public static void ShowError(MetroWindow win, string message)
        {
            SimpleMessageBox("Error", win, message);
        }
        public static void ShowInfo(MetroWindow win, string message)
        {
            SimpleMessageBox("Information", win, message);
        }

        public static void ShowWarning(object context, string message)
        {
            SimpleMessageBox(context, "Warning", message);
        }

        public static void ShowError(object context, string message)
        {
            SimpleMessageBox(context, "Error", message);
        }

        public static void ShowInfo(object context, string message)
        {
            SimpleMessageBox(context, "Information", message);
        }

        public static async void SimpleMessageBox(string type, MetroWindow win, string message)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK",
                //NegativeButtonText = "OK",
                //FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogColorScheme.Accented// win.MetroDialogOptions.ColorScheme
            };

            await win.ShowMessageAsync(type, message, MessageDialogStyle.Affirmative, mySettings);
        }

        public static async void SimpleMessageBox(object context, string type, string message)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK",
                //NegativeButtonText = "OK",
                //FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogColorScheme.Accented// win.MetroDialogOptions.ColorScheme
            };

            await DialogCoordinator.Instance.ShowMessageAsync(context, type, message, MessageDialogStyle.Affirmative,
                mySettings);
        }

        public static async Task<MessageDialogResult> ShowYesNo(string title, MetroWindow win, string message)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                ColorScheme = MetroDialogColorScheme.Accented
            };

            return await win.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, mySettings);
        }

        public static async Task<MessageDialogResult> ShowYesNo(string title, object context, string message)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                ColorScheme = MetroDialogColorScheme.Accented
            };

            return await DialogCoordinator.Instance.ShowMessageAsync(context, title, message, MessageDialogStyle.AffirmativeAndNegative, mySettings);
        }
    }
}
