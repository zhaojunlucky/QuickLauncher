using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Utility.Win32.Api
{
    public class Win32Api
    {
        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, IntPtr lpdwResult);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public static void FlashWindow(Window window, FlashWindowFlags flags, uint count = 0, uint timeout = 0)
        {
            // Get the native window handle (HWND) from the WPF window
            WindowInteropHelper helper = new WindowInteropHelper(window);
            IntPtr hWnd = helper.Handle;

            if (hWnd == IntPtr.Zero)
            {
                // Window handle not yet available (e.g., window not loaded/shown)
                // You might want to handle this or defer the call.
                return;
            }

            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = flags;
            fInfo.uCount = count;
            fInfo.dwTimeout = timeout;

            FlashWindowEx(ref fInfo);
        }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static int NetStatisticsGet(
            [MarshalAs(UnmanagedType.LPWStr)] string serverName,
            [MarshalAs(UnmanagedType.LPWStr)] string service,
            int level,
            int options,
            out IntPtr BufPtr);

        [StructLayout(LayoutKind.Sequential)]
        public struct STAT_WORKSTATION_0
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 StatisticsStartTime;

            public long BytesReceived;
            public long SmbsReceived;
            public long PagingReadBytesRequested;
            public long NonPagingReadBytesRequested;
            public long CacheReadBytesRequested;
            public long NetworkReadBytesRequested;

            public long BytesTransmitted;
            public long SmbsTransmitted;
            public long PagingWriteBytesRequested;
            public long NonPagingWriteBytesRequested;
            public long CacheWriteBytesRequested;
            public long NetworkWriteBytesRequested;

            public int InitiallyFailedOperations;
            public int FailedCompletionOperations;

            public int ReadOperations;
            public int RandomReadOperations;
            public int ReadSmbs;
            public int LargeReadSmbs;
            public int SmallReadSmbs;

            public int WriteOperations;
            public int RandomWriteOperations;
            public int WriteSmbs;
            public int LargeWriteSmbs;
            public int SmallWriteSmbs;

            public int RawReadsDenied;
            public int RawWritesDenied;

            public int NetworkErrors;

            //  Connection/Session counts
            public int Sessions;
            public int FailedSessions;
            public int Reconnects;
            public int CoreConnects;
            public int Lanman20Connects;
            public int Lanman21Connects;
            public int LanmanNtConnects;
            public int ServerDisconnects;
            public int HungSessions;
            public int UseCount;
            public int FailedUseCount;

            public int CurrentCommands;

        }

        /// Define our Constants we will use
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;

        public const int WM_HOTKEY = 0x0312;

        // FlashWindowEx flags
        [Flags]
        public enum FlashWindowFlags : uint
        {
            /// <summary>Stop flashing. The system restores the window to its original state.</summary>
            FLASHW_STOP = 0,
            /// <summary>Flash the window caption.</summary>
            FLASHW_CAPTION = 1,
            /// <summary>Flash the taskbar button.</summary>
            FLASHW_TRAY = 2,
            /// <summary>Flash both the window caption and taskbar button.</summary>
            FLASHW_ALL = (FLASHW_CAPTION | FLASHW_TRAY),
            /// <summary>Flash continuously, until the FLASHW_STOP flag is set.</summary>
            FLASHW_TIMER = 4,
            /// <summary>Flash continuously until the window comes to the foreground.</summary>
            FLASHW_TIMERNOFG = 12
        }

        // Structure for FlashWindowEx
        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;    // The size of the structure in bytes.
            public IntPtr hwnd;    // A Handle to the window to be flashed.
            public FlashWindowFlags dwFlags; // The flash status.
            public uint uCount;    // The number of times to flash the window.
            public uint dwTimeout; // The rate in milliseconds at which the window is to be flashed.
        }

    }

}
