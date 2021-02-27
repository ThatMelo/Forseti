using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ForsetiFramework
{
    public static class Extensions
    {
        public static async Task ReactOk(this ModuleBase<SocketCommandContext> c)
        {
            await c.Context.Message.AddReactionAsync(new Emoji("👌"));
        }

        public static async Task ReactError(this ModuleBase<SocketCommandContext> c)
        {
            await c.Context.Message.AddReactionAsync(new Emoji("❌"));
        }
    }
}
