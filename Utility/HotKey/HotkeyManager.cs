using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Utility.Win32.Api;
using System.Windows.Interop;

namespace Utility.HotKey
{
    [Flags]
    public enum HotKeyModifiers
    {
        None = 0,
        Alt = 1,            // MOD_ALT
        Control = 2,        // MOD_CONTROL
        Shift = 4,          // MOD_SHIFT
        WindowsKey = 8,     // MOD_WIN
    }

    public class HotkeyManager
    {
        
        private Dictionary<string, HotKey> nameHotKey = new Dictionary<string, HotKey>();
        private Dictionary<int, string> idHotkeyName = new Dictionary<int, string>();
        internal static readonly IntPtr HwndMessage = (IntPtr)(-3);
        private HwndSource hwndSource;

        public bool IsEnabled { get; set; } = true;

        public static HotkeyManager Current { get { return LazyInitializer.Instance; } }

        private static class LazyInitializer
        {
            static LazyInitializer() { }
            public static readonly HotkeyManager Instance = new HotkeyManager();
        }

        private HotkeyManager()
        {
            var parameters = new HwndSourceParameters("Hotkey sink")
            {
                HwndSourceHook = WndProc,
                ParentWindow = HwndMessage
            };
            hwndSource = new HwndSource(parameters);
            
        }

        public void RegisterHotKey(string name, uint fsModifiers, uint vk, EventHandler<HotkeyEventArgs> handler)
        {
            var hotKey = new HotKey(hwndSource.Handle, name, fsModifiers, vk, handler);
            lock(nameHotKey)
            {
                UnRegisterHotKey(name);
                nameHotKey.Add(name, hotKey);
                idHotkeyName.Add(hotKey.Id, name);
                hotKey.RegisterHotKey();
            }
        }

        public void RegisterHotKey(string name, HotKeyModifiers fsModifiers, int vk, EventHandler<HotkeyEventArgs> handler)
        {
            RegisterHotKey(name, (uint)fsModifiers, (uint)vk, handler);
        }

        public void UnRegisterHotKey(string name)
        {
            HotKey hotKey;
            if (nameHotKey.TryGetValue(name, out hotKey))
            {
                hotKey.UnRegisterHotKey();
            }
        }

        public bool HandleHotKey(IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled)
            {
                return false;
            }

            int id = wParam.ToInt32();
            string name;
            if (idHotkeyName.TryGetValue(id, out name))
            {
                var hotkey = nameHotKey[name];
                var handler = hotkey.Handler;
                if (handler != null)
                {
                    var e = new HotkeyEventArgs(name);
                    handler(this, e);
                }
            }
            return true;
        }

        public void UnRegisterHotKeys()
        {
            lock(nameHotKey)
            {
                foreach(var kv in nameHotKey)
                {
                    kv.Value.UnRegisterHotKey();
                    idHotkeyName.Remove(kv.Value.Id);
                    nameHotKey.Remove(kv.Key);
                }
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Api.WM_HOTKEY)
            {
                handled = HandleHotKey(wParam, lParam);
            }

            return IntPtr.Zero;
        }
    }
}
