using QuickLauncher.Model;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickLauncher.Misc
{
    public class PreDefinedCommand
    {
        public int Version { get; set; }

        [JsonConverter(typeof(JsonListConverter<QuickCommand>))]
        public List<QuickCommand> QuickCommands { get; set; }

        public static PreDefinedCommand LoadFile(string file)
        {
            return LoadJson(File.ReadAllText(file));
        }

        public static PreDefinedCommand LoadJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonListConverter<QuickCommand>() },
            };
            return JsonSerializer.Deserialize<PreDefinedCommand>(json, options);
        }
    }
}
