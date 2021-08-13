using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickLauncher.Misc
{
    public class UpgradeSQL
    { 
        [JsonConverter(typeof(JsonListConverter<List<string>>))]
        public List<List<string>> SQLS { get; set; }

        public static UpgradeSQL loadFile(string file)
        {
            return loadJson(File.ReadAllText(file));
        }

        public static UpgradeSQL loadJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonListConverter<List<string>>() },
            };
            return JsonSerializer.Deserialize<UpgradeSQL>(json, options);
        }
    }
}
