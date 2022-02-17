using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickLauncher.Misc
{
    public class UpgradeSql
    {
        [JsonConverter(typeof(JsonListConverter<List<string>>))]
        public List<List<string>> Sqls { get; set; }

        public static UpgradeSql LoadFile(string file)
        {
            return LoadJson(File.ReadAllText(file));
        }

        public static UpgradeSql LoadJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonListConverter<List<string>>() },
            };
            return JsonSerializer.Deserialize<UpgradeSql>(json, options);
        }
    }
}
