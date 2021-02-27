using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Forseti.Modules
{
    public class Tags : ModuleBase<SocketCommandContext>
    {
        public static async Task<Tag[]> GetTags()
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(Config.Path + "tags.json")) { return null; }
                return JsonConvert.DeserializeObject<Tag[]>(File.ReadAllText(Config.Path + "tags.json"));
            });
        }

        public static async Task<Tag> GetTag(string name)
        {
            var tags = await GetTags();
            return tags?.FirstOrDefault(t => t.Name == name.ToLower());
        }

        public static async Task SetTag(params Tag[] t)
        {
            var tagsArr = await GetTags();
            var tags = tagsArr is null ? new List<Tag>() : tagsArr.ToList();
            // Remove old
            t.ToList().ForEach(t2 => tags.RemoveAll(t3 => t3.Name == t2.Name));
            t.ToList().ForEach(t2 => tags.Add(t2));
            File.WriteAllText(Config.Path + "tags.json", JsonConvert.SerializeObject(tags.ToArray()));
        }

        public static async Task RemoveTag(string name)
        {
            var tagsArr = await GetTags();
            if (tagsArr is null) { return; }
            var tags = tagsArr.ToList();
            tags.RemoveAll(t => t.Name == name);
            File.WriteAllText(Config.Path + "tags.json", JsonConvert.SerializeObject(tags.ToArray()));
        }

        [Command("tag")]
        [RequireRole("staff")]
        public async Task Tag(string name, [Remainder]string con = "")
        {
            if (con == "" && Context.Message.Attachments.Count == 0)
            {
                await RemoveTag(name);
            }
            else
            {
                var t = new Tag()
                {
                    Name = name.ToLower(),
                    Content = con,
                    AttachmentURLs = Context.Message.Attachments.Select(a => a.Url).ToArray(),
                };
                await SetTag(t);
            }
            await Context.Message.AddReactionAsync(new Emoji("👌"));
        }
    }

    [Serializable]
    public class Tag
    {
        public string Name;
        public string Content;
        public string[] AttachmentURLs;
    }
}
