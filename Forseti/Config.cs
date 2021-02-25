using System;
using System.IO;
using Newtonsoft.Json;

namespace Forseti
{
    [Serializable]
    public class Config
    {
        [JsonProperty("token")]
        public string Token;
        [JsonProperty("mod-logs")]
        public ulong ModLogsID;

        public static Config Load(string filePath) => 
            JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));
    }
}
