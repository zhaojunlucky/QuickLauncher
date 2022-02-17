using QuickLauncher.Misc;
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
        private static volatile GlobalSetting _instance;
        private static readonly object SyncRoot = new Object();
        private PreDefinedCommand preDefinedCommand;

        public static GlobalSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        _instance ??= new GlobalSetting();
                    }
                }

                return _instance;
            }
        }

        public PreDefinedCommand GetPreDefinedCommand()
        {
            return preDefinedCommand ?? (preDefinedCommand = LoadDefaultCommands());
        }

        public UpgradeSql GetUpgradeSql()
        {
            return LoadUpgradeSql();
        }

        private PreDefinedCommand LoadDefaultCommands()
        {
            PreDefinedCommand result = null;
            string fileName = "DefaultCommands.json";
            var streamInfo = ReadAppFile(fileName);
            if (streamInfo == null)
            {
                streamInfo = ReadAppResource(fileName);
            }

            if (streamInfo != null)
            {
                StreamReader sr = new StreamReader(streamInfo.Stream);
                try
                {
                    result = PreDefinedCommand.LoadJson(sr.ReadToEnd());
                }
                catch (JsonException e)
                {
                    Debug.WriteLine(e);
                }

                if (result == null)
                {
                    result = new PreDefinedCommand
                    {
                        Version = 0,
                        QuickCommands = new System.Collections.Generic.List<QuickCommand>()
                    };
                }
            }

            return result;
        }

        private UpgradeSql LoadUpgradeSql()
        {
            UpgradeSql result = null;
            string fileName = "Upgrade.json";
            var streamInfo = ReadAppFile(fileName) ?? ReadAppResource(fileName);

            if (streamInfo != null)
            {
                StreamReader sr = new StreamReader(streamInfo.Stream);
                try
                {
                    result = UpgradeSql.LoadJson(sr.ReadToEnd());
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

        private StreamResourceInfo ReadAppFile(string fileName)
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

        private StreamResourceInfo ReadAppResource(string fileName)
        {
            Uri uri = new Uri($"/Resources/{fileName}", UriKind.Relative);
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
