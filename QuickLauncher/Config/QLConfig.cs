﻿using System;
using System.IO;

namespace QuickLauncher.Config
{
    public class QlConfig
    {
        internal static string AppConfigBaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unicorn\\QuickLancher");

#if DEBUG
        internal static string DbFileName = "lancherDb-debug.db";
        internal static string Singleton = "QuickLauncher-debug";
#else
        internal static string DbFileName = "lancherDb.db";
        internal static string Singleton = "QuickLauncher";
#endif
        internal static string DbFilePath = Path.Combine(AppConfigBaseDir, DbFileName);
        internal static string DbConnStr = "Data Source =" + DbFilePath;
        internal static string LogFile = Path.Combine(AppConfigBaseDir, Singleton + ".log");

    }
}
