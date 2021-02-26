using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ProfanityFilter;

namespace Forseti.Commands
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        static string[] BadWords = File.ReadAllText(@"C:\Forseti\badwords.txt").Replace("\r", "").Split('\n');

        static SocketTextChannel ModLogs => BotManager.Instance.Client.GetChannel(814327531216961616) as SocketTextChannel;
        static SocketTextChannel General => BotManager.Instance.Client.GetChannel(814328175881355304) as SocketTextChannel;
        // User Info, This Is Fine, Warn & Delete, Mute & Delete
        static Emoji[] CardReactions = new[] { "👤", "✅", "⚠", "🔇" }.Select(s => new Emoji(s)).ToArray();

        static Moderation()
        {
            BotManager.Instance.Client.MessageReceived += async (msg) => { if (msg is SocketUserMessage msg2) { await CheckMessage(msg2); } };
            BotManager.Instance.Client.MessageUpdated += async (a, msg, c) => { if (msg is SocketUserMessage msg2) { await CheckMessage(msg2); } };
            BotManager.Instance.Client.UserBanned += async (usr, guild) => await ModLogs.SendMessageAsync($"{usr.Mention} ({usr.Id}) has been banned.");
            BotManager.Instance.Client.UserUnbanned += async (usr, guild) => await ModLogs.SendMessageAsync($"{usr.Mention} ({usr.Id}) has been unbanned.");
            BotManager.Instance.Client.UserLeft += async (usr) => await ModLogs.SendMessageAsync($"{usr.Mention} has left or was kicked.");

            BotManager.Instance.Client.UserJoined += async (usr) =>
            {
                await usr.AddRoleAsync(usr.Guild.Roles.First(r => r.Name == "Member"));
                await General.SendMessageAsync($"Welcome to the server, {usr.Mention}! Please make sure to read through <#814326414618132521>.");
            };

            BotManager.Instance.Client.ReactionAdded += Client_ReactionAdded;
        }

        private static async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> cMsg, ISocketMessageChannel ch, SocketReaction r)
        {
            if (r.User.Value.IsBot) { return; } // Ignore bot reactions
            if (ch.Id != ModLogs.Id) { return; } // Make sure reaction was in mod-logs
            var msg = await ch.GetMessageAsync(cMsg.Id) as IUserMessage;
            if (msg.Embeds.Count <= 0) { return; } // Make sure the message has an embed
            var embed = msg.Embeds.First();
            if (embed.Color == Color.Green || embed.Fields.Any(f => f.Name == "Resolved")) { return; } // Make sure it's not already resolved

            var usrId = ulong.Parse(embed.Fields.First(f => f.Name == "User ID").Value);
            var usr = (ch as SocketGuildChannel).Guild.GetUser(usrId);

            async Task Resolve(string type)
            {
                await msg.ModifyAsync(m =>
                {
                    var e = msg.Embeds.First().ToEmbedBuilder()
                        .AddField("Resolved", type, false)
                        .WithColor(Color.Green);
                    m.Embed = new Optional<Embed>(e.Build());
                });
            }

            async Task<(SocketGuildChannel channel, IMessage message)> GetInfo(string url)
            {
                var parts = url.Replace(")", "").Split('/');
                Console.WriteLine(string.Join(" | ", parts));
                var messageId = ulong.Parse(parts[parts.Length - 1]);
                var channelId = ulong.Parse(parts[parts.Length - 2]);

                var channel = BotManager.Instance.Client.GetChannel(channelId) as SocketGuildChannel;
                var message = await (channel as ITextChannel).GetMessageAsync(messageId);
                return (channel, message);
            }

            if (r.Emote.Name == CardReactions[0].Name)
            {
                await User.PostUserInfo(usr, ch as SocketTextChannel);
            }
            else if (r.Emote.Name == CardReactions[1].Name)
            {
                await Resolve("Marked as OK by " + r.User.Value.Mention);
            }
            else if (r.Emote.Name == CardReactions[2].Name)
            {
                await Resolve("Marked as *Warn & Delete* by " + r.User.Value.Mention);

                var url = embed.Fields.First(f => f.Name == "Jump To Post").Value;
                var i = await GetInfo(url);

                await usr.SendMessageAsync($"{r.User.Value.Mention} has given you a warning for posting inappropriate language, " +
                    $"links, or other material.\n```{i.message.Content}\n``` ({i.message.Attachments.Count} attachment(s))");

                if (!(i.message is null))
                {
                    await i.message.DeleteAsync();
                }
            }
            else if (r.Emote.Name == CardReactions[3].Name)
            {
                await Resolve("Marked as *Mute & Delete* by " + r.User.Value.Mention);

                var url = embed.Fields.First(f => f.Name == "Jump To Post").Value;
                var i = await GetInfo(url);

                if (!usr.Roles.Any(r => r.Name == "Muted"))
                {
                    await usr.RemoveRoleAsync(i.channel.Guild.Roles.First(r => r.Name == "Member"));
                    await usr.AddRoleAsync(i.channel.Guild.Roles.First(r => r.Name == "Muted"));
                    await usr.SendMessageAsync($"You have been muted by {r.User.Value.Mention}.");
                    await ModLogs.SendMessageAsync($"{usr.Mention} was muted by {r.User.Value.Mention}.");
                }

                if (!(i.message is null))
                {
                    await i.message.DeleteAsync();
                }
            }
        }

        public static async Task CheckMessage(SocketUserMessage m)
        {
            if (m.Author.IsBot || m.Content is null) { return; }

            var guild = (m.Channel as SocketGuildChannel).Guild;

            var e = new EmbedBuilder()
                .WithTitle("Forseti Entry")
                .WithDescription(m.Content)
                .WithAuthor(m.Author)
                .WithColor(Color.Orange)
                .WithCurrentTimestamp()
                .AddField("Channel", $"<#{m.Channel.Id}>", true)
                .AddField("Jump To Post", @$"[Link](https://discord.com/channels/{guild.Id}/{m.Channel.Id}/{m.Id})", true)
                .AddField("User ID", m.Author.Id, true);

            // Strict, auto-delete
            var clearedContent = Regex.Replace(m.Content, "[^a-zA-Z1-9 -_]", string.Empty);
            var clearedParts = clearedContent.Split(new[] { " ", "-", "_" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var b2 in clearedParts)
            {
                foreach (var b in BadWords)
                {
                    if (b2.Equals(b))
                    {
                        e.AddField("Flagged", b, true);
                        e.AddField("Auto-Deleted", "Yes", true);
                        e.Color = Color.Red;
                        e.Url = "";
                        await m.DeleteAsync();
                        await CreateModCard(e);
                        return;
                    }
                }
            }

            // Softer, don't delete

            //clearedContent = Regex.Replace(m.Content.ToLower(), "[^a-z]", string.Empty);
            //foreach (var b in BadWords)
            //{
            //    if (clearedContent.Contains(b.Replace(" ", "")))
            //    {
            //        e.AddField("Flagged", b, true);
            //        e.AddField("Auto-Deleted", "No", true);
            //        await CreateModCard(e);
            //        return;
            //    }
            //}

            var filter = new ProfanityFilter.ProfanityFilter();
            filter.AddProfanity("bird");
            var list = filter.DetectAllProfanities(m.Content);
            if (list.Count > 0)
            {
                e.AddField("Flagged", string.Join(", ", list), true);
                e.AddField("Auto-Deleted", "No", true);
                await CreateModCard(e);
                return;
            }
        }

        private static async Task CreateModCard(EmbedBuilder e)
        {
            var eM = await ModLogs.SendMessageAsync(embed: e.Build());
            _ = Task.Run(async () =>
            {
                await eM.AddReactionsAsync(CardReactions);
            });
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
