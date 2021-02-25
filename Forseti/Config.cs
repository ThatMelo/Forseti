using System;
using System.IO;
using Newtonsoft.Json;

namespace Forseti
{
    [Serializable]
    public class Config
    {
        public string Token;

        public static Config Load(string filePath) => 
            JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));
    }
}
