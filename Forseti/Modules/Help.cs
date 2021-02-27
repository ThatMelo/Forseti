using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Forseti;

namespace Forseti.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task GetHelp()
        {
            var builders = new List<EmbedBuilder>();

            var last = false;
            void checkBuilders(bool commands /*false if tags*/)
            {
                if (builders.Count == 0 || builders.Last().Fields.Count >= 25 || last != commands)
                {
                    builders.Add(new EmbedBuilder()
                        .WithTitle(commands ? "Commands" : "Tags")
                        .WithColor(commands ? Color.Blue : Color.Green)
                        .WithDescription($"These are the {(commands ? "commands" : "tags")} you can run."));
                    return;
                }
                last = commands;
            }

            foreach (var module in BotManager.Instance.Commands.Modules)
            {
                foreach (var command in module.Commands)
                {
                    checkBuilders(true);

                    //var syntax = command.Attributes.FirstOrDefault(a => a is Forseti.);

                    var sumString = $"{string.Join(", ", command.Aliases)}\n{command.Summary}\n{""}";
                    builders.Last().AddField(command.Name, sumString);
                }
            }

            var builder = new EmbedBuilder()
            {
                Color = Color.Blue,
                Description = "These are the commands you can use:"
            };

            foreach (var module in BotManager.Instance.Commands.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        description += $"{Config.Prefix}{cmd.Aliases.First()}\n";
                    }
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
