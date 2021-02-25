using System;
using System.Threading.Tasks;
using Discord;

namespace Forseti
{
    public class LoggingService
    {
        public async Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
        }
    }
}
