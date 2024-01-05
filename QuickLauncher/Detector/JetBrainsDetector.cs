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
using System.IO.Compression;

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

            var stateJsonPath = Path.Combine(toolboxPath, "state.json");

            DetectApplicationBinary(stateJsonPath);

        }



        private void DetectApplicationBinary(string stateJson)
        {

            if (!File.Exists(stateJson))
            {
                return;
            }
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(stateJson));
            if (document.RootElement.TryGetProperty("tools", out JsonElement tools))
            {
                if (tools.ValueKind != JsonValueKind.Array)
                {
                    return;
                }
            }
            foreach(JsonElement prod in tools.EnumerateArray())
            {
                Trace.WriteLine(prod.ToString());
                try
                {
                    if (!prod.TryGetProperty("displayName", out JsonElement displayNameEl)) {
                        Trace.TraceError($"invalid displayName");
                        continue; 
                    }
                    var displayName = displayNameEl.GetString();

                    if (!prod.TryGetProperty("installLocation", out JsonElement installLocationEl))
                    {
                        Trace.TraceError($"invalid installLocation");
                        continue;
                    }
                    var installLocation = installLocationEl.GetString();

                    if (!prod.TryGetProperty("launchCommand", out JsonElement launchCommandEl))
                    {
                        Trace.TraceError($"invalid launchCommand");
                        continue;
                    }
                    var launchCommand = launchCommandEl.GetString();
                    var workDir = Path.Combine(installLocation, "jbr\\bin");
                    if (!Directory.Exists(workDir))
                    {
                        workDir = installLocation;
                    }


                    var uuid = GuidUtil.Create(GuidUtil.UrlNamespace, $"{Category.ToLower()}-{displayName.ToLower()}").ToString();
                    var quickCommand = new QuickCommand
                    {
                        Path = Path.Combine(installLocation, launchCommand),
                        WorkDirectory = workDir,
                        Alias = displayName,
                        Uuid = uuid,
                        IsReadOnly = true,
                        IsAutoStart = AutoDetectCommandAutoStartMgr.Current.IsAutoStart(uuid)
                    };
                    quickCommands.Add(quickCommand);
                } catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
                
            }

        }
    }
}
