using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Forseti.Commands
{
    public class User : ModuleBase<SocketCommandContext>
    {
        public static async Task PostUserInfo(SocketGuildUser usr, SocketTextChannel channel)
        {
            var roles = usr.Roles.Select(r => r.Name).Where(r => !r.Contains("everyone"));

            var e = new EmbedBuilder()
                .WithTitle(usr.Username + "#" + usr.Discriminator)
                .AddField("ID", usr.Id, true)
                .AddField("Joined", usr.JoinedAt, true)
                .AddField("Account Created", usr.CreatedAt, true)
                .AddField("Roles", string.Join(", ", roles), true)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(usr.GetAvatarUrl());
            await channel.SendMessageAsync(embed: e.Build());
        }

        [Command("info")]
        public async Task Info(SocketGuildUser usr = null)
        {
            usr = usr is null ? Context.Message.Author as SocketGuildUser : usr;
            await PostUserInfo(usr, Context.Channel as SocketTextChannel);
        }
    }
}
