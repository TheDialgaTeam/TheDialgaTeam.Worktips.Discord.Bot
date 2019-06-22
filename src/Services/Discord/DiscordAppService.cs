using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Discord;
using TheDialgaTeam.Worktips.Discord.Bot.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Discord
{
    public class DiscordAppService : IInitializable, IDisposable
    {
        public DiscordAppClient DiscordAppClient { get; private set; }

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
            if (Program.CancellationTokenSource.IsCancellationRequested)
                return;

            DiscordAppClient = new DiscordAppClient(ConfigService.BotToken, new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            DiscordAppClient.Log += DiscordAppClientOnLog;
            DiscordAppClient.ShardReady += DiscordAppClientOnShardReady;
            DiscordAppClient.MessageReceived += DiscordAppClientOnMessageReceived;

            InitializeAsync().GetAwaiter().GetResult();

            Program.TasksToAwait.Add(Task.Factory.StartNew(async () =>
            {
                while (!Program.CancellationTokenSource.IsCancellationRequested)
                {
                    await DiscordAppClient.UpdateAsync().ConfigureAwait(false);
                    await Task.Delay(1000).ConfigureAwait(false);
                }
            }, Program.CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current).Unwrap());
        }

        private async Task InitializeAsync()
        {
            if (Program.CancellationTokenSource.IsCancellationRequested)
                return;

            await DiscordAppClient.DiscordAppLoginAsync().ConfigureAwait(false);
            await DiscordAppClient.DiscordAppStartAsync().ConfigureAwait(false);
        }

        private Task DiscordAppClientOnLog(DiscordAppClient discordAppClient, LogMessage logMessage)
        {
            Task.Run(async () =>
            {
                var botId = discordAppClient.DiscordShardedClient?.CurrentUser?.Id;
                var botName = discordAppClient.DiscordShardedClient?.CurrentUser?.ToString();
                var message = discordAppClient.DiscordShardedClient?.CurrentUser == null ? $"[Bot] {logMessage.ToString()}" : $"[Bot {botId}] {botName}: {logMessage.ToString()}";

                switch (logMessage.Severity)
                {
                    case LogSeverity.Critical:
                        await LoggerService.LogMessageAsync(message, ConsoleColor.Red).ConfigureAwait(false);
                        break;

                    case LogSeverity.Error:
                        await LoggerService.LogMessageAsync(message, ConsoleColor.Red).ConfigureAwait(false);
                        break;

                    case LogSeverity.Warning:
                        await LoggerService.LogMessageAsync(message, ConsoleColor.Yellow).ConfigureAwait(false);
                        break;

                    case LogSeverity.Info:
                        await LoggerService.LogMessageAsync(message).ConfigureAwait(false);
                        break;

                    case LogSeverity.Verbose:
                        await LoggerService.LogMessageAsync(message).ConfigureAwait(false);
                        break;

                    case LogSeverity.Debug:
                        await LoggerService.LogMessageAsync(message, ConsoleColor.Cyan).ConfigureAwait(false);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        private Task DiscordAppClientOnShardReady(DiscordAppClient discordAppClient, DiscordSocketClient discordSocketClient)
        {
            Task.Run(async () =>
            {
                await discordSocketClient.SetGameAsync($"@{discordAppClient.DiscordShardedClient.CurrentUser.Username} help").ConfigureAwait(false);
                await LoggerService.LogMessageAsync($"{discordAppClient.DiscordShardedClient.CurrentUser}: Shard {discordSocketClient.ShardId + 1}/{discordAppClient.DiscordShardedClient.Shards.Count} is ready!", ConsoleColor.Green).ConfigureAwait(false);

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
                    await LoggerService.LogErrorMessageAsync(ex).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        private Task DiscordAppClientOnMessageReceived(DiscordAppClient discordAppClient, SocketMessage socketMessage)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!(socketMessage is SocketUserMessage socketUserMessage))
                        return;

                    ICommandContext context = new ShardedCommandContext(discordAppClient.DiscordShardedClient, socketUserMessage);
                    var argPos = 0;

                    if (socketUserMessage.Channel is SocketDMChannel)
                    {
                        socketUserMessage.HasMentionPrefix(discordAppClient.DiscordShardedClient.CurrentUser, ref argPos);
                        socketUserMessage.HasStringPrefix(ConfigService.BotPrefix, ref argPos, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        if (!socketUserMessage.HasMentionPrefix(discordAppClient.DiscordShardedClient.CurrentUser, ref argPos) &&
                            !socketUserMessage.HasStringPrefix(ConfigService.BotPrefix, ref argPos, StringComparison.OrdinalIgnoreCase))
                            return;
                    }

                    await Program.CommandService.ExecuteAsync(context, argPos, Program.ServiceProvider).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await LoggerService.LogErrorMessageAsync(ex).ConfigureAwait(false);
                }
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