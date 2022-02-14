using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using QuickLauncher.Misc;
using Utility;

namespace QuickLauncher.Detector
{
    internal class JetBrainsDetector : IDetector
    {
        private readonly List<QuickCommand> quickCommands = new List<QuickCommand>();
        private readonly string jetbrainsPath = @"JetBrains\Toolbox";
        public string Category => "JetBrains";

        public IList<QuickCommand> QuickCommands => quickCommands;

        public void Detect()
        {
            QuickCommands.Clear();
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var toolboxPath = Path.Combine(basePath, jetbrainsPath);
            if (!Directory.Exists(toolboxPath))
            {
                Trace.TraceWarning($"Jetbrains folder {toolboxPath} doesn't exist");
                return;
            }

            var settingJsonPath = Path.Combine(toolboxPath, ".settings.json");
            string appPath = ReadSettingJson(settingJsonPath);

            appPath ??= Path.Combine(toolboxPath, "apps");

            if (!Directory.Exists(appPath))
            {
                return;
            }

            Trace.TraceInformation($"Jetbrains folder {appPath}");
            Detect(appPath);

        }

        private void Detect(string fullPath)
        {
            foreach (var product in Directory.GetDirectories(fullPath))
            {
                Trace.TraceInformation($"Checking {product}");
                var folderInfo = new DirectoryInfo(product);

                DetectProduct(folderInfo.Name, product);
            }
        }

        private void DetectProduct(string productName, string productPath)
        {
            var subDirs = Directory.GetDirectories(productPath, "ch-*");
            if (subDirs.Length == 0)
            {
                Trace.TraceWarning($"{productPath} doesn't have application installed");
                return;
            }
            Array.Sort(subDirs);
            var activePath = subDirs.Last();
            DetectApplication(productName, activePath);
        }

        private void DetectApplication(string productName, string activePath)
        {
            Regex rx = new Regex(@"^\d+(.\d+)+\.\d+$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var appFolders = new List<string>();
            foreach (var folder in Directory.GetDirectories(activePath))
            {
                var folderInfo = new DirectoryInfo(folder);
                if (!rx.IsMatch(folderInfo.Name))
                {
                    continue;
                }
                appFolders.Add(folder);
            }

            if (appFolders.Count == 0)
            {
                return;
            }
            appFolders.Sort();
            var appFolder = appFolders.Last();
            DetectApplicationBinary(productName, appFolder);
        }

        private void DetectApplicationBinary(string productName, string appFolder)
        {
            string appFile = null;
            var workingDir = Path.Combine(appFolder, "bin");
            if (!Directory.Exists(workingDir))
            {
                Trace.TraceError($"Invalid path {workingDir}");
                return;
            }

            var exeFiles = Directory.GetFiles(workingDir, "*64.exe");
            if (exeFiles.Length == 1)
            {
                appFile = exeFiles[0];
            }
            else if(exeFiles.Length > 1)
            {
                foreach (var exe in exeFiles)
                {
                    var exeInfo = new FileInfo(exe);
                    var name = exeInfo.Name.TrimEnd("64.exe".ToCharArray());
                    if (!productName.Contains(name))
                    {
                        continue;
                    }
                    appFile = exe;
                    break;
                }
            }

            if (appFile == null)
            {
                Trace.TraceWarning($"Unable to find application for {productName}");
                return;
            }

            Trace.TraceInformation($"Find application {appFile}");
            var uuid = GuidUtil.Create(GuidUtil.UrlNamespace, $"{Category.ToLower()}-{productName.ToLower()}").ToString();
            var quickCommand = new QuickCommand
            {
                Path = appFile,
                WorkDirectory = workingDir,
                Alias = productName,
                Uuid = uuid,
                IsReadOnly = true,
                IsAutoStart = AutoDetectCommandAutoStartMgr.Current.IsAutoStart(uuid)
            };
            quickCommands.Add(quickCommand);

        }

        private string ReadSettingJson(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("install_location", out JsonElement installLocation))
            {
                if (installLocation.ValueKind == JsonValueKind.String)
                {
                    return installLocation.GetString();
                }
            }

            return null;
        }
    }
}
