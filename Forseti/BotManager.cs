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
            Logger = new LoggingService();

            Config = Config.Load(@"C:\Forseti\config.json");
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LargeThreshold = 250,
                LogLevel = LogSeverity.Debug,
                RateLimitPrecision = RateLimitPrecision.Millisecond,
            });

            Commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Debug,
                SeparatorChar = ' ',
                ThrowOnError = false,
            });

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
            if (!(msg.HasCharPrefix('!', ref argPos) || 
                msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) || msg.Author.IsBot) { return; }

            var context = new SocketCommandContext(Client, msg);
            await Commands.ExecuteAsync(context, argPos, null);
        }

        private async Task Client_Ready()
        {

        }
    }
}
