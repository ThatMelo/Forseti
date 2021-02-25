using System.Threading.Tasks;

namespace Forseti
{
    public class BotManager
    {
        public Config Config;

        public async Task Start()
        {
            Config = Config.Load(@"C:\Forseti\config.json");


        }
    }
}
