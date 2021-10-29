using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

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
