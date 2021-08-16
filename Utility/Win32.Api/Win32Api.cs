using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Win32.Api
{
    public class Win32Api
    {
        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(System.IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(System.IntPtr hWnd);

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

        delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern uint GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern uint GetWindowText(IntPtr hWnd, StringBuilder lpString, uint nMaxCount);
        static bool IsApplicationWindow(IntPtr hWnd)
        {
            return (GetWindowLong(hWnd, GWL_EXSTYLE) & WS_EX_APPWINDOW) != 0;
        }
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsProc ewp, int lParam);

        public static IntPtr GetWindowHandle(int pid, string title)
        {
            var result = IntPtr.Zero;

            EnumWindowsProc enumerateHandle = delegate (IntPtr hWnd, int lParam)
            {
                int id;
                GetWindowThreadProcessId(hWnd, out id);

                if (pid == id)
                {
                    var clsName = new StringBuilder(256);
                    var hasClass = GetClassName(hWnd, clsName, 256);
                    if (hasClass)
                    {

                        var maxLength = (int)GetWindowTextLength(hWnd);
                        var builder = new StringBuilder(maxLength + 1);
                        GetWindowText(hWnd, builder, (uint)builder.Capacity);

                        var text = builder.ToString();
                        var className = clsName.ToString();

                        // There could be multiple handle associated with our pid, 
                        // so we return the first handle that satisfy:
                        // 1) the handle title/ caption matches our window title,
                        // 2) the window class name starts with HwndWrapper (WPF specific)
                        // 3) the window has WS_EX_APPWINDOW style

                        if (title == text && className.StartsWith("HwndWrapper") && IsApplicationWindow(hWnd))
                        {
                            result = hWnd;
                            return false;
                        }
                    }
                }
                return true;
            };

            EnumDesktopWindows(IntPtr.Zero, enumerateHandle, 0);

            return result;
        }

        /// Define our Constants we will use
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;
        public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        public const int WM_SETTINGCHANGE = 0x1a;
        public const int SMTO_ABORTIFHUNG = 0x0002;
        public const uint WM_SHOWWINDOW = 0x0018;
        public const int SW_PARENTOPENING = 3;
        const int GWL_EXSTYLE = (-20);
        const uint WS_EX_APPWINDOW = 0x40000;
    }

}
