using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ForsetiFramework.Modules
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        [Command("poll")]
        [RequireRole("staff")]
        [Summary("Create a poll for users to vote on. (Max 9 items)")]
        [Syntax("poll <name>\n<item1>\n<item2>\n[item3]\n...")]
        public async Task Poll([Remainder]string suffix)
        {
            var items = suffix.Split('\n');
            if (items.Length < 3) { await this.ReactError(); return; }
            if (items.Length > 10) { await this.ReactError(); return; }
            await Context.Message.DeleteAsync();

            // Most places don't render this right (including in Visual Studio), 
            // but these are the unicode keycap digits. 1⃣ is the same as :one: in Discord.
            var nums = "1⃣ 2⃣ 3⃣ 4⃣ 5⃣ 6⃣ 7⃣ 8⃣ 9⃣".Split(' ');

            var pollTitle = items[0];
            var pollItems = items.Skip(1).ToList();
            var itemStrings = pollItems.Select(p => $"{nums[pollItems.IndexOf(p)]} {p}");

            var e = new EmbedBuilder()
                .WithTitle(pollTitle)
                .WithDescription(string.Join("\n", itemStrings))
                .WithColor(Color.Green);
            var m = await ReplyAsync(embed: e.Build());
            for (var i = 0; i < pollItems.Count; i++)
            {
                await m.AddReactionAsync(new Emoji(nums[i]));
            }
        }
    }
}
