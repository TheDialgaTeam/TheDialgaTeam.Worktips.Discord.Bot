using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.mscorlib.System.Threading.Tasks;
using TheDialgaTeam.Worktips.Discord.Bot.Discord;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Discord
{
    public class DiscordAppService : IInitializable, IBackgroundLoopingTask, IDisposable
    {
        public DiscordAppClient DiscordAppClient { get; private set; }

        public BackgroundLoopingTask RunningTask { get; private set; }

        private Program Program { get; }

        private LoggerService LoggerService { get; }

        private SettingService SettingService { get; }

        public DiscordAppService(Program program, LoggerService loggerService, SettingService settingService)
        {
            Program = program;
            LoggerService = loggerService;
            SettingService = settingService;
        }

        public void Initialize()
        {
            DiscordAppClient = new DiscordAppClient(SettingService.BotToken, new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            DiscordAppClient.Log += DiscordAppClientOnLog;
            DiscordAppClient.ShardReady += DiscordAppClientOnShardReady;
            DiscordAppClient.MessageReceived += DiscordAppClientOnMessageReceived;
            DiscordAppClient.DiscordAppLoginAsync().GetAwaiter().GetResult();
            DiscordAppClient.DiscordAppStartAsync().GetAwaiter().GetResult();

            RunningTask = Task.Factory.StartNewBackgroundLoopingTask(async _ => { await DiscordAppClient.UpdateAsync().ConfigureAwait(false); }, Program.CancellationTokenSource.Token);
        }

        private Task DiscordAppClientOnLog(DiscordAppClient discordAppClient, LogMessage logMessage)
        {
            Task.Run(() =>
            {
                var botId = discordAppClient.DiscordShardedClient?.CurrentUser?.Id;
                var botName = discordAppClient.DiscordShardedClient?.CurrentUser?.ToString();
                var message = discordAppClient.DiscordShardedClient?.CurrentUser == null ? $"[Bot] {logMessage.ToString()}" : $"[Bot {botId}] {botName}: {logMessage.ToString()}";

                switch (logMessage.Severity)
                {
                    case LogSeverity.Critical:
                        LoggerService.LogMessage(message, ConsoleColor.Red);
                        break;

                    case LogSeverity.Error:
                        LoggerService.LogMessage(message, ConsoleColor.Red);
                        break;

                    case LogSeverity.Warning:
                        LoggerService.LogMessage(message, ConsoleColor.Yellow);
                        break;

                    case LogSeverity.Info:
                        LoggerService.LogMessage(message);
                        break;

                    case LogSeverity.Verbose:
                        LoggerService.LogMessage(message);
                        break;

                    case LogSeverity.Debug:
                        LoggerService.LogMessage(message, ConsoleColor.Cyan);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            return Task.CompletedTask;
        }

        private Task DiscordAppClientOnShardReady(DiscordAppClient discordAppClient, DiscordSocketClient discordSocketClient)
        {
            Task.Run(async () =>
            {
                await discordSocketClient.SetGameAsync($"@{discordAppClient.DiscordShardedClient.CurrentUser.Username} help").ConfigureAwait(false);
                LoggerService.LogMessage($"{discordAppClient.DiscordShardedClient.CurrentUser}: Shard {discordSocketClient.ShardId + 1}/{discordAppClient.DiscordShardedClient.Shards.Count} is ready!", ConsoleColor.Green);
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        private Task DiscordAppClientOnMessageReceived(DiscordAppClient discordAppClient, SocketMessage socketMessage)
        {
            Task.Run(async () =>
            {
                if (!(socketMessage is SocketUserMessage socketUserMessage))
                    return;

                ICommandContext context = new ShardedCommandContext(discordAppClient.DiscordShardedClient, socketUserMessage);
                var argPos = 0;

                if (socketUserMessage.Channel is SocketDMChannel)
                    socketUserMessage.HasMentionPrefix(discordAppClient.DiscordShardedClient.CurrentUser, ref argPos);
                else
                {
                    if (!socketUserMessage.HasMentionPrefix(discordAppClient.DiscordShardedClient.CurrentUser, ref argPos) &&
                        !socketUserMessage.HasStringPrefix(SettingService.BotPrefix, ref argPos, StringComparison.OrdinalIgnoreCase))
                        return;
                }

                await Program.CommandService.ExecuteAsync(context, argPos, Program.ServiceProvider).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (DiscordAppClient == null)
                return;

            DiscordAppClient.DiscordAppStopAsync().GetAwaiter().GetResult();
            DiscordAppClient.DiscordAppLogoutAsync().GetAwaiter().GetResult();

            DiscordAppClient.Log -= DiscordAppClientOnLog;
            DiscordAppClient.ShardReady -= DiscordAppClientOnShardReady;

            DiscordAppClient.Dispose();
        }
    }
}