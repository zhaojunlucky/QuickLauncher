using QuickLauncher.Model;
using QuickLauncher.Notification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Utility.HotKey;

namespace QuickLauncher.Misc
{
    internal class HotKeyUtil
    {
        public static void UnRegisterHotKeys()
        {
            try
            {
                HotkeyManager.Current.UnRegisterHotKeys();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        public static void RegisterHotKey(SettingItem hotKeyItem, EventHandler<HotkeyEventArgs> handler)
        {
            try
            {
                if (hotKeyItem.Value.Trim().Length == 0)
                {
                    Trace.TraceInformation("skip register hotkey {0}, empty hotkey value", hotKeyItem.Key);
                }
                else
                {
                    HotkeyManager.Current.RegisterHotKey(hotKeyItem.Key, hotKeyItem.Value, handler);
                }

                //HotkeyManager.Current.RegisterHotKey("open main window", HotKeyModifiers.Control | HotKeyModifiers.Alt, KeyInterop.VirtualKeyFromKey(Key.Q), OnOpenWindowHotKey);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                ToastNotificationUtil.SendNotification("Failed to register HotKey", e.Message);
            }
        }
    }
}
