using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
using QuickLauncher.Misc;
using Utility;

namespace QuickLauncher.Detector
{
    internal class MicrosoftDetector : IDetector
    {
        private readonly IList<QuickCommand> quickCommands = new List<QuickCommand>();

        private static readonly List<Regex> ProductPatterns = new List<Regex>()
        {
            new Regex("^word$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^access", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^excel", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^onenote", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^outlook", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^PowerPoint", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex("^Publisher", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^Visual Studio \d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };
        public string Category => "Microsoft";

        public IList<QuickCommand> QuickCommands => quickCommands;

        public void Detect()
        {
            QuickCommands.Clear();
            IWshShell wsh = new WshShell();
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            var fullPath = Path.Combine(basePath, "Programs");
            if (!Directory.Exists(fullPath))
            {
                return;
            }

            foreach (var lnkPath in Directory.GetFiles(fullPath, "*.lnk"))
            {
                var fileInfo = new FileInfo(lnkPath);
                var name = fileInfo.Name.TrimEnd(fileInfo.Extension.ToCharArray());
                
                foreach (var regex in ProductPatterns)
                {
                    if (regex.IsMatch(name))
                    {
                        IWshShortcut sc = (IWshShortcut)wsh.CreateShortcut(lnkPath);
                        var workingDir = sc.WorkingDirectory;
                        if (string.IsNullOrEmpty(workingDir))
                        {
                            workingDir = Path.GetDirectoryName(sc.TargetPath);
                        }

                        var uuid = GuidUtil.Create(GuidUtil.UrlNamespace, $"{Category.ToLower()}-{name.ToLower()}")
                            .ToString();
                        var quickCommand = new QuickCommand
                        {
                            Path = sc.TargetPath,
                            WorkDirectory = workingDir,
                            Alias = name,
                            Uuid = uuid,
                            Command = sc.Arguments,
                            IsReadOnly = true,
                            IsAutoStart = AutoDetectCommandAutoStartMgr.Current.IsAutoStart(uuid)
                        };
                        quickCommands.Add(quickCommand);
                    }
                }
            }
        }
    }
}
