using System;
using System.Threading.Tasks;
using TheDialgaTeam.Cryptonote.Rpc.Worktips;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc
{
    public sealed class RpcService : IInitializable, IDisposable
    {
        public DaemonRpcClient DaemonRpcClient { get; private set; }

        public WalletRpcClient WalletRpcClient { get; private set; }

        private Program Program { get; }

        private LoggerService LoggerService { get; }

        private ConfigService ConfigService { get; }

        private DateTimeOffset NextCheck { get; set; }

        public RpcService(Program program, LoggerService loggerService, ConfigService configService)
        {
            Program = program;
            LoggerService = loggerService;
            ConfigService = configService;
        }

        public void Initialize()
        {
            if (ConfigService.DaemonPort != null)
            {
                if (!string.IsNullOrWhiteSpace(ConfigService.DaemonPasswordProxyHeader) && !string.IsNullOrWhiteSpace(ConfigService.DaemonPassword))
                    DaemonRpcClient = new DaemonRpcClient(ConfigService.DaemonHost, ConfigService.DaemonPort.Value, null, null, null, client => client.DefaultRequestHeaders.Add(ConfigService.DaemonPasswordProxyHeader, ConfigService.DaemonPassword));
                else
                    DaemonRpcClient = new DaemonRpcClient(ConfigService.DaemonHost, ConfigService.DaemonPort.Value, ConfigService.DaemonUsername, ConfigService.DaemonPassword);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(ConfigService.DaemonPasswordProxyHeader) && !string.IsNullOrWhiteSpace(ConfigService.DaemonPassword))
                    DaemonRpcClient = new DaemonRpcClient(ConfigService.DaemonHost, null, null, null, client => client.DefaultRequestHeaders.Add(ConfigService.DaemonPasswordProxyHeader, ConfigService.DaemonPassword));
                else
                    DaemonRpcClient = new DaemonRpcClient(ConfigService.DaemonHost, ConfigService.DaemonUsername, ConfigService.DaemonPassword);
            }

            if (ConfigService.WalletPort != null)
            {
                if (!string.IsNullOrWhiteSpace(ConfigService.WalletPasswordProxyHeader) && !string.IsNullOrWhiteSpace(ConfigService.WalletPassword))
                    WalletRpcClient = new WalletRpcClient(ConfigService.WalletHost, ConfigService.WalletPort.Value, null, null, null, client => client.DefaultRequestHeaders.Add(ConfigService.WalletPasswordProxyHeader, ConfigService.WalletPassword));
                else
                    WalletRpcClient = new WalletRpcClient(ConfigService.WalletHost, ConfigService.WalletPort.Value, ConfigService.WalletUsername, ConfigService.WalletPassword);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(ConfigService.WalletPasswordProxyHeader) && !string.IsNullOrWhiteSpace(ConfigService.WalletPassword))
                    WalletRpcClient = new WalletRpcClient(ConfigService.WalletHost, null, null, null, client => client.DefaultRequestHeaders.Add(ConfigService.WalletPasswordProxyHeader, ConfigService.WalletPassword));
                else
                    WalletRpcClient = new WalletRpcClient(ConfigService.WalletHost, ConfigService.WalletUsername, ConfigService.WalletPassword);
            }

            Program.TasksToAwait.Add(Task.Factory.StartNew(async () =>
            {
                NextCheck = DateTimeOffset.Now.AddMinutes(15);

                while (!Program.CancellationTokenSource.IsCancellationRequested)
                {
                    if (DateTimeOffset.Now < NextCheck)
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                        continue;
                    }

                    try
                    {
                        await LoggerService.LogMessageAsync("Attempting to save the wallet...").ConfigureAwait(false);
                        await WalletRpcClient.StoreAsync(Program.CancellationTokenSource.Token).ConfigureAwait(false);
                        await LoggerService.LogMessageAsync("Save complete!", ConsoleColor.Green).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await LoggerService.LogErrorMessageAsync(ex).ConfigureAwait(false);
                    }
                    finally
                    {
                        NextCheck = DateTimeOffset.Now.AddMinutes(15);
                    }
                }
            }, Program.CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current).Unwrap());
        }

        public void Dispose()
        {
            DaemonRpcClient?.Dispose();
            WalletRpcClient?.Dispose();
        }
    }
}