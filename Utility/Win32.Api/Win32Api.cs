using System;
using System.Runtime.InteropServices;

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
    }

}
