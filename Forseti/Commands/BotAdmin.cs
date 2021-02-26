using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Forseti.Commands
{
    public class BotAdmin : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong! " + Context.Client.Latency + "ms");
        }

        [Command("stop")]
        [RequireOwner]
        public async Task Stop()
        {
            await Context.Message.AddReactionAsync(new Emoji("👌"));
            await BotManager.Instance.Client.StopAsync();
            Environment.Exit(0);
        }

        [Command("testerror")]
        [RequireOwner]
        public async Task TestError()
        {
            Console.WriteLine("Throwing Test Error");
            throw new Exception("Test Error!");
        }
    }
}
