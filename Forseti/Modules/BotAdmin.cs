using System;
using System.Diagnostics;
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

        [Command("restart")]
        [RequireOwner]
        public async Task Restart(bool update = true)
        {
            await Context.Message.AddReactionAsync(new Emoji("👌"));
            await BotManager.Instance.Client.StopAsync();

            if (!Config.Debug && update)
            {
                Process.Start("update.bat");
            }
            else if (!Config.Debug && !update)
            {
                Process.Start("restart.bat");
            }
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
