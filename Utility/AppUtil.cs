using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using Utility.Win32.Api;

namespace Utility
{
    public class AppUtil
    {
        public static bool IsRunAsAdmin()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var claims = principal.Claims;
            return (claims.FirstOrDefault(c => c.Value == "S-1-5-32-544") != null);
        }

        public static string GetAppFullPath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }


        public static void RestartApp(string allowedCaller, string arguments = "", [CallerMemberName] string callerName = "")
        {
            Application.Current.Exit += (s, e) =>
            {
                if (callerName == allowedCaller)
                {
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName, arguments);
                }
            };
            Application.Current.Shutdown();
        }

        public static DateTime GetSystemLastRebootTime()
        {
            IntPtr bufPtr = IntPtr.Zero;
            int val = Win32Api.NetStatisticsGet(null, "LanmanWorkstation", 0, 0, out bufPtr);
            Win32Api.STAT_WORKSTATION_0 wks = new Win32Api.STAT_WORKSTATION_0();
            if (val == 0)
            {
                wks = (Win32Api.STAT_WORKSTATION_0)Marshal.PtrToStructure(bufPtr, typeof(Win32Api.STAT_WORKSTATION_0));
            }
            DateTime rebootTime = DateTime.FromFileTime(wks.StatisticsStartTime);
            return rebootTime;
        }
    }
}
