using QuickLauncher.Misc;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickLauncher.Miscs
{
    public class PreDefinedCommand
    {
        public int Version { get; set; }

        [JsonConverter(typeof(JsonListConverter<QuickCommand>))]
        public List<QuickCommand> QuickCommands { get; set; }

        public static PreDefinedCommand loadFile(string file)
        {
            return loadJson(File.ReadAllText(file));
        }

        public static PreDefinedCommand loadJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonListConverter<QuickCommand>() },
            };
            return JsonSerializer.Deserialize<PreDefinedCommand>(json, options);
        }
    }
}
