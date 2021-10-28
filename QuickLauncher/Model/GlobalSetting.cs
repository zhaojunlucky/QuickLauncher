using QuickLauncher.Misc;
using QuickLauncher.Miscs;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Resources;

namespace QuickLauncher.Model
{
    public sealed class GlobalSetting
    {
        private static volatile GlobalSetting instance;
        private static object syncRoot = new Object();
        private PreDefinedCommand preDefinedCommand = null;

        public static GlobalSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new GlobalSetting();
                    }
                }

                return instance;
            }
        }

        public PreDefinedCommand GetPreDefinedCommand()
        {
            if (preDefinedCommand == null)
            {
                preDefinedCommand = loadDefaultCommands();
            }
            return preDefinedCommand;
        }

        public UpgradeSQL GetUpgradeSQL()
        {
            return loadUpgradeSQL();
        }

        private PreDefinedCommand loadDefaultCommands()
        {
            PreDefinedCommand result = null;
            string fileName = "DefaultCommands.json";
            var streamInfo = readAppFile(fileName);
            if (streamInfo == null)
            {
                streamInfo = readAppResource(fileName);
            }

            if (streamInfo != null)
            {
                StreamReader sr = new StreamReader(streamInfo.Stream);
                try
                {
                    result = PreDefinedCommand.loadJson(sr.ReadToEnd());
                }
                catch (JsonException e)
                {
                    Debug.WriteLine(e);
                }
                
                if (result == null)
                {
                    result = new PreDefinedCommand();
                    result.Version = 0;
                    result.QuickCommands = new System.Collections.Generic.List<QuickCommand>();
                }
            }
            
            return result;
        }

        private UpgradeSQL loadUpgradeSQL()
        {
            UpgradeSQL result = null;
            string fileName = "Upgrade.json";
            var streamInfo = readAppFile(fileName);
            if (streamInfo == null)
            {
                streamInfo = readAppResource(fileName);
            }

            if (streamInfo != null)
            {
                StreamReader sr = new StreamReader(streamInfo.Stream);
                try
                {
                    result = UpgradeSQL.loadJson(sr.ReadToEnd());
                }
                catch (JsonException e)
                {
                    Trace.TraceError(e.Message);
                    Trace.TraceError(e.StackTrace);
                    Debug.WriteLine(e);
                }
            }

            return result;
        }

        private StreamResourceInfo readAppFile(string fileName)
        {
            Uri uri = new Uri(fileName, UriKind.Relative);
            try
            {
                return Application.GetRemoteStream(uri);
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }

        private StreamResourceInfo readAppResource(string fileName)
        {
            Uri uri = new Uri(string.Format("/Resources/{0}", fileName), UriKind.Relative);
            try
            {
                return Application.GetResourceStream(uri);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }
    }
}
