

namespace QuickLauncher.Notification
{
    class ToastNotificationUtil
    {
        public static void SendNotification(string title, string message)
        {
            // Uno Platform does not support Windows toast notifications on non-UWP targets.
            // As a workaround, show a simple WPF MessageBox instead.
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}
