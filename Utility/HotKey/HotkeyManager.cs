using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Utility.Win32.Api;
using System.Windows.Interop;
using System.Windows.Input;

namespace Utility.HotKey
{
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

        public void RegisterHotKey(string name, ModifierKeys fsModifiers, int vk, EventHandler<HotkeyEventArgs> handler)
        {
            RegisterHotKey(name, (uint)fsModifiers, (uint)vk, handler);
        }

        public void RegisterHotKey(string name, string combineKeys, EventHandler<HotkeyEventArgs> handler)
        {
            var rawHotKey = RawHotKey.Parse(combineKeys);
            if (rawHotKey.Key != Key.None || rawHotKey.HotKeyModifiers != ModifierKeys.None)
            {
                RegisterHotKey(name, rawHotKey.HotKeyModifiers, KeyInterop.VirtualKeyFromKey(rawHotKey.Key), handler);
            }
        }

        public void UnRegisterHotKey(string name)
        {
            HotKey hotKey;
            if (nameHotKey.TryGetValue(name, out hotKey))
            {
                hotKey.UnRegisterHotKey();
                nameHotKey.Remove(name);
                idHotkeyName.Remove(hotKey.Id);
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
