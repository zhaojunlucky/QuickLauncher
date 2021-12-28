using System;
using System.Runtime.InteropServices;
using Utility.Win32.Api;

namespace Utility.HotKey
{
    internal class HotKey
    {
        private static int _nextHotKeyId = 9000;
        private readonly int id;
        private readonly uint fsModifiers;
        private readonly uint vk;
        private readonly EventHandler<HotkeyEventArgs> handler;
        private readonly IntPtr hwnd;
        private readonly string name;

        internal HotKey(IntPtr hwnd, string name, uint fsModifiers, uint virtualKey, EventHandler<HotkeyEventArgs> handler)
        {
            this.hwnd = hwnd;
            this.name = name;
            id = ++_nextHotKeyId;
            this.fsModifiers = fsModifiers;
            vk = virtualKey;
            this.handler = handler;

        }

        public int Id
        {
            get { return id; }
        }

        public uint VirtualKey
        {
            get { return vk; }
        }

        public uint FsModifiers
        {
            get { return fsModifiers; }
        }

        public EventHandler<HotkeyEventArgs> Handler
        {
            get { return handler; }
        }

        public void RegisterHotKey()
        {
            if (!Win32Api.RegisterHotKey(hwnd, id, fsModifiers, vk))
            {
                var hr = Marshal.GetHRForLastWin32Error();
                var ex = Marshal.GetExceptionForHR(hr);
                if ((uint)hr == 0x80070581)
                    throw new HotkeyAlreadyRegisteredException(name + " is already registered.", ex);
                throw ex;
            }
        }

        public void UnRegisterHotKey()
        {
            if (!Win32Api.UnregisterHotKey(hwnd, Id))
            {
                var hr = Marshal.GetHRForLastWin32Error();
                throw Marshal.GetExceptionForHR(hr);
            }

        }
    }
}
