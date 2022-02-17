using Microsoft.Toolkit.Uwp.Notifications;

namespace QuickLauncher.Notification
{
    class ToastNotificationUtil
    {
        public static void SendNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
    }
}
