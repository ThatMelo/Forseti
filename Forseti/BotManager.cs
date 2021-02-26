using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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
                DefaultRunMode = RunMode.Sync,
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
            if (!(arg is SocketUserMessage msg)) { return; }

            var argPos = 0;

            // Make sure it's prefixed (with ! or bot mention), and that caller isn't a bot
            if (!(msg.HasStringPrefix(Config.Prefix, ref argPos) || 
                msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) || msg.Author.IsBot) { return; }

            var commandLog = Client.GetChannel(814970218555768882) as SocketTextChannel;
            var mC = $"{DateTimeOffset.UtcNow} > {arg.Author.Username}#{arg.Author.Discriminator} > `{arg.Content}` - <#{arg.Channel.Id}>";
            var m = await commandLog.SendMessageAsync(mC);

            var context = new SocketCommandContext(Client, msg);
            var result = await Commands.ExecuteAsync(context, argPos, null);

            await m.ModifyAsync(m2 => m2.Content = result.IsSuccess ? "✅ " + mC : "❌ " + mC);
        }

        private async Task Client_Ready()
        {
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
 