using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Utility
{
    public class AutoStartUtil
    {
        public static void SetAutoStart(bool on)
        {
            if (on)
            {
                string name = Process.GetCurrentProcess().MainModule.ModuleName;
                name = name[..name.LastIndexOf(".", StringComparison.CurrentCulture)];
                SetAutoStart(GetAutoStartShortCutDir(), name, AppUtil.GetAppFullPath());
            }
            else
            {
                CancelAutoStart(GetAutoStartShortCutDir(), AppUtil.GetAppFullPath());
            }
        }

        private static void CancelAutoStart(string startupDir, string appFullName)
        {
            var shortCuts = GetAppAutoStartShortCut(startupDir, appFullName);
            foreach (var f in shortCuts)
            {
                FileAttributes attr = System.IO.File.GetAttributes(f);
                if (attr == FileAttributes.Directory)
                {
                    Directory.Delete(f, true);
                }
                else
                {
                    System.IO.File.Delete(f);
                }
            }
        }

        private static void SetAutoStart(string directory, string shortcutName, string targetPath,
            string description = null, string iconLocation = null)
        {
            if (IsAutoStart(directory, targetPath))
            {
                return;
            }
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string shortcutPath = Path.Combine(directory, $"{shortcutName}.lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.WindowStyle = 1;
            shortcut.Description = description;
            shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;
            shortcut.Save();
        }

        public static bool IsAutoStart()
        {
            return IsAutoStart(GetAutoStartShortCutDir(), AppUtil.GetAppFullPath());
        }

        public static bool IsAutoStart(string startupDir, string appFullName)
        {
            return GetAppAutoStartShortCut(startupDir, appFullName).Count > 0;
        }

        public static List<string> GetAppAutoStartShortCut(string startupDir, string appFullName)
        {
            var shortCuts = new List<string>();
            string[] files = Directory.GetFiles(startupDir, "*.lnk");
            foreach (var f in files)
            {
                if (appFullName.Equals(GetAppPathFromShortCut(f), StringComparison.OrdinalIgnoreCase))
                {
                    shortCuts.Add(f);
                }
            }
            return shortCuts;
        }

        private static string GetAppPathFromShortCut(string shortcutPath)
        {
            if (System.IO.File.Exists(shortcutPath))
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                return shortcut.TargetPath;
            }
            else
            {
                return "";
            }
        }

        public static string GetAutoStartShortCutDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        }
    }
}
