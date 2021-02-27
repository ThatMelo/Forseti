using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Forseti.Modules;

namespace Forseti
{
    public class BotManager
    {
        public static BotManager Instance;
        public BotManager() { Instance = this; }

        public Config Config;
        public DiscordSocketClient Client;
        public LoggingService Logger;
        public CommandService Commands;

        public async Task Start()
        {
            Config = Config.Load(Config.Path + "config.json");
            Logger = new LoggingService();
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LargeThreshold = 250,
                LogLevel = LogSeverity.Warning,
                RateLimitPrecision = RateLimitPrecision.Millisecond,
            });

            Commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Warning,
                SeparatorChar = ' ',
                ThrowOnError = false,
            });

            Commands.Log += Logger.Client_Log;
            Client.Log += Logger.Client_Log;
            Client.MessageReceived += HandleCommands;
            Client.Ready += Client_Ready;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task HandleCommands(SocketMessage arg)
        {
            try
            {
                if (!(arg is SocketUserMessage msg)) { return; }

                var argPos = 0;

                // Make sure it's prefixed (with ! or bot mention), and that caller isn't a bot
                if (!(msg.HasStringPrefix(Config.Prefix, ref argPos) ||
                    msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) || msg.Author.IsBot) { return; }
                var context = new SocketCommandContext(Client, msg);

                var tagName = arg.Content.Substring(argPos, arg.Content.Length - argPos).Split(' ')[0].ToLower();
                var tag = await Tags.GetTag(tagName);
                if (tag is null) // Normal command handling
                {
                    var commandLog = Client.GetChannel(814970218555768882) as SocketTextChannel;
                    var mC = $"{DateTimeOffset.UtcNow} > {arg.Author.Username}#{arg.Author.Discriminator} > `{arg.Content}` - <#{arg.Channel.Id}>";
                    var m = await commandLog.SendMessageAsync(mC);

                    var result = await Commands.ExecuteAsync(context, argPos, null);

                    await m.ModifyAsync(m2 => m2.Content = result.IsSuccess ? "✅ " + mC : "❌ " + mC);
                    return;
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Tag command handling
                        if (!(tag.Content is null || tag.Content == string.Empty))
                        {
                            var index = argPos + tagName.Length + (argPos + tagName.Length == msg.Content.Length ? 0 : 1);
                            var suffix = msg.Content.Substring(index, msg.Content.Length - index);
                            await arg.Channel.SendMessageAsync(tag.Content.Replace("{author}", msg.Author.Mention).Replace("{suffix}", suffix));
                        }
                        if (!(tag.AttachmentURLs is null || tag.AttachmentURLs.Length == 0))
                        {
                            foreach (var a in tag.AttachmentURLs)
                            {
                                await arg.Channel.SendMessageAsync(a);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        await Logger.Client_Log(new LogMessage(LogSeverity.Critical, "BotManager's Tag Handler", e.Message, e));
                    }
                });
            }
            catch (Exception e)
            {
                await Logger.Client_Log(new LogMessage(LogSeverity.Critical, "BotManager's MessageHandler", e.Message, e));
            }
        }

        private async Task Client_Ready()
        {
            if (Config.Debug) { return; }
            var botTesting = Client.GetChannel(814330280969895936) as SocketTextChannel;
            var e = new EmbedBuilder()
                .WithAuthor(Client.CurrentUser)
                .WithTitle("Bot Ready!")
                .WithCurrentTimestamp()
                .WithColor(Color.Teal);
            await botTesting.SendMessageAsync(embed: e.Build());
        }
    }
}
