using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;

namespace ForsetiFramework
{
    public class LoggingService
    {
        public DiscordWebhookClient ErrorsClient;

        public LoggingService()
        {
            ErrorsClient = new DiscordWebhookClient(BotManager.Instance.Config.ErrorWebhookUrl);
            ErrorsClient.Log += Client_Log;
            Console.WriteLine("Created webhook client.");
        }

        public async Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);

            if (!(arg.Exception is null))
            {
                // Errors to ignore
                if (arg.Exception is GatewayReconnectException) { return; }
                if (arg.Exception.Message.Equals("WebSocket connection was closed")) { return; }

                var color = arg.Severity == LogSeverity.Critical ? Color.Red : Color.Orange;

                if (arg.Exception is CommandException ex)
                {

                    var e = new EmbedBuilder()
                        .WithColor(color)
                        .WithCurrentTimestamp()
                        .WithTitle(ex.InnerException.GetType().Name)
                        .AddField("User", ex.Context.User.Username + "#" + ex.Context.User.Discriminator, true)
                        .AddField("Location", ex.Context.Guild.Name + " > " + ex.Context.Channel.Name, true)
                        .AddField("Command", ex.Context.Message.Content, true)
                        .AddField("Exception Message", ex.InnerException.Message)
                        .AddField("Exception Stack Trace", "```\n" + ex.InnerException.StackTrace.Replace("\n", "\n ") + "\n```");

                    await ErrorsClient.SendMessageAsync(embeds: new[] { e.Build() });
                }
                else
                {
                    var e = new EmbedBuilder()
                        .WithColor(color)
                        .WithCurrentTimestamp()
                        .WithTitle(arg.Exception.GetType().Name)
                        .AddField("Exception Message", arg.Exception.Message)
                        .AddField("Exception Stack Trace", "```\n" + arg.Exception.StackTrace.Replace("\n", "\n ") + "\n```");

                    await ErrorsClient.SendMessageAsync(embeds: new[] { e.Build() });
                }
            }
            else
            {
                var e = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp()
                    .WithTitle(arg.Message)
                    .AddField("Severity", arg.Severity);

                await ErrorsClient.SendMessageAsync(embeds: new[] { e.Build() });
            }
        }
    }
}
