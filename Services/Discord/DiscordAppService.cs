using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.mscorlib.System.Threading.Tasks;
using TheDialgaTeam.Worktips.Discord.Bot.Discord;
using TheDialgaTeam.Worktips.Discord.Bot.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Discord
{
    public class DiscordAppService : IInitializable, IBackgroundLoopingTask, IDisposable
    {
        public DiscordAppClient DiscordAppClient { get; private set; }

        public BackgroundLoopingTask RunningTask { get; private set; }

        private Program Program { get; }

        private LoggerService LoggerService { get; }

        private ConfigService ConfigService { get; }

        private RpcService RpcService { get; }

        private SqliteDatabaseService SqliteDatabaseService { get; }

        public DiscordAppService(Program program, LoggerService loggerService, ConfigService configService, RpcService rpcService, SqliteDatabaseService sqliteDatabaseService)
        {
            Program = program;
            LoggerService = loggerService;
            ConfigService = configService;
            RpcService = rpcService;
            SqliteDatabaseService = sqliteDatabaseService;
        }

        public void Initialize()
        {
            DiscordAppClient = new DiscordAppClient(ConfigService.BotToken, new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            DiscordAppClient.Log += DiscordAppClientOnLog;
            DiscordAppClient.ShardReady += DiscordAppClientOnShardReady;
            DiscordAppClient.MessageReceived += DiscordAppClientOnMessageReceived;

            InitializeAsync().GetAwaiter().GetResult();

            RunningTask = Task.Factory.StartNewBackgroundLoopingTask(async _ => { await DiscordAppClient.UpdateAsync().ConfigureAwait(false); }, Program.CancellationTokenSource.Token);
        }

        private async Task InitializeAsync()
        {
            if (Program.CancellationTokenSource.IsCancellationRequested)
                return;

            try
            {
                await DiscordAppClient.DiscordAppLoginAsync().ConfigureAwait(false);
                await DiscordAppClient.DiscordAppStartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LoggerService.LogErrorMessage(ex);
                Program.CancellationTokenSource.Cancel();
            }
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

                try
                {
                    using (var context = SqliteDatabaseService.GetContext())
                    {
                        if (context.WalletAccountTable.Count(a => a.UserId == discordAppClient.DiscordShardedClient.CurrentUser.Id) == 0)
                        {
                            var address = await RpcService.WalletRpcClient.GetAddressAsync(0).ConfigureAwait(false);

                            context.WalletAccountTable.Add(new WalletAccount
                            {
                                UserId = discordAppClient.DiscordShardedClient.CurrentUser.Id,
                                AccountIndex = 0,
                                TipWalletAddress = address.Address
                            });

                            await context.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogErrorMessage(ex);
                }
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
                        !socketUserMessage.HasStringPrefix(ConfigService.BotPrefix, ref argPos, StringComparison.OrdinalIgnoreCase))
                        return;
                }

                await Program.CommandService.ExecuteAsync(context, argPos, Program.ServiceProvider).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (DiscordAppClient.IsLoggedIn || DiscordAppClient.IsStarted)
                DiscordAppClient?.Dispose();
        }
    }
}