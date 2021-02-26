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
        [JsonProperty("errorsWebhook")]
        public string ErrorWebhookUrl;

        public static Config Load(string filePath) => 
            JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));
    }
}
