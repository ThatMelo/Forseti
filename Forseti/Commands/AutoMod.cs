using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Forseti.Commands
{
    public class AutoMod : ModuleBase<SocketCommandContext>
    {
        static SocketTextChannel ModLogs => BotManager.Instance.Client.GetChannel(BotManager.Instance.Config.ModLogsID) as SocketTextChannel;
        static SocketTextChannel General => BotManager.Instance.Client.GetChannel(814328175881355304) as SocketTextChannel;
        string[] CardReactions = new[] { "👤", "✅", "⚠", "⛔", "🛑", "🔇" };

        static AutoMod()
        {
            BotManager.Instance.Client.MessageReceived += Client_MessageReceived;
            BotManager.Instance.Client.MessageUpdated += Client_MessageUpdated;

            BotManager.Instance.Client.UserBanned += async (usr, guild) =>
                await ModLogs.SendMessageAsync($"{usr.Mention} ({usr.Id}) has been banned.");

            BotManager.Instance.Client.UserUnbanned += async (usr, guild) =>
                await ModLogs.SendMessageAsync($"{usr.Mention} ({usr.Id}) has been unbanned.");

            BotManager.Instance.Client.UserLeft += async (usr) =>
                await ModLogs.SendMessageAsync($"{usr.Mention} has left or was kicked.");

            BotManager.Instance.Client.UserJoined += async (usr) =>
            {
                await usr.AddRoleAsync(usr.Guild.Roles.First(r => r.Name == "Member"));
                await General.SendMessageAsync($"Welcome to the server, {usr.Mention}! Please make sure to read through <#814326414618132521>.");
            };
        }

        private static async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (arg2 is SocketUserMessage msg) { CheckMessage(msg); }
        }

        private static async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage msg) { CheckMessage(msg); }
        }

        public static void CheckMessage(SocketUserMessage m)
        {
            var clearedContent = Regex.Replace(m.Content.ToLower(), "[^a-z]", string.Empty);
            var banned = File.ReadAllText(@"C:\Forseti\banned.json").Split('\n');
            foreach (var b in banned)
            {
                if (clearedContent.Contains(b))
                {

                }
            }
        }

        [Command("kick")]
        [RequireRole("staff")]
        public async Task Kick(SocketGuildUser user, [Remainder]string reason = "violating the rules")
        {
            reason = reason.EndsWith(".") ? reason : reason + ".";
            await user.SendMessageAsync($"You have been kicked from {Context.Guild.Name} for {reason}.");
            await user.KickAsync();
            await ModLogs.SendMessageAsync($"{user.Mention} was kicked by {Context.User}.");
            await Context.Message.DeleteAsync();
        }

        [Command("ban")]
        [RequireRole("staff")]
        public async Task Ban(SocketGuildUser user, [Remainder]string reason = "violating the rules")
        {
            reason = reason.EndsWith(".") ? reason : reason + ".";
            await user.SendMessageAsync($"You have been banned from {Context.Guild.Name} for {reason}");
            await user.BanAsync(0, reason);
            await Context.Message.DeleteAsync();
        }

        [Command("unban")]
        [Alias("pardon")]
        [RequireRole("staff")]
        public async Task Unban(ulong user)
        {
            await Context.Guild.RemoveBanAsync(user);
            await ModLogs.SendMessageAsync($"{user} unbanned by {Context.User.Mention}.");
            await Context.Message.DeleteAsync();
        }

        [Command("mute")]
        [RequireRole("staff")]
        public async Task Mute(SocketGuildUser user)
        {
            if (!user.Roles.Any(r => r.Name == "Muted"))
            {
                await user.RemoveRoleAsync(Context.Guild.Roles.First(r => r.Name == "Member"));
                await user.AddRoleAsync(Context.Guild.Roles.First(r => r.Name == "Muted"));
                await user.SendMessageAsync($"You have been muted by {Context.User.Mention}.");
                await ModLogs.SendMessageAsync($"{user.Mention} was muted by {Context.User.Mention}.");
                await Context.Message.DeleteAsync();
            }
        }

        [Command("unmute")]
        [RequireRole("staff")]
        public async Task Unmute(SocketGuildUser user)
        {
            if (user.Roles.Any(r => r.Name == "Muted"))
            {
                await user.AddRoleAsync(Context.Guild.Roles.First(r => r.Name == "Member"));
                await user.RemoveRoleAsync(Context.Guild.Roles.First(r => r.Name == "Muted"));
                await user.SendMessageAsync($"You have been unmuted by {Context.User.Mention}.");
                await ModLogs.SendMessageAsync($"{user.Mention} was unmuted by {Context.User.Mention}.");
                await Context.Message.DeleteAsync();
            }
        }

        [Command("purge")]
        [RequireRole("staff")]
        public async Task Purge(int count)
        {
            var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            await ModLogs.SendMessageAsync($"{count} messages purged by {Context.User.Mention} in #{Context.Channel.Name}.");
        }
    }
}
