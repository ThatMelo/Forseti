using System;
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

        [Command("gotobed")]
        [Alias("stop")]
        [RequireOwner]
        public async Task GotToBed()
        {
            await Context.Message.AddReactionAsync(new Emoji("👌"));
            await BotManager.Instance.Client.StopAsync();
            Environment.Exit(0);
        }
    }
}
