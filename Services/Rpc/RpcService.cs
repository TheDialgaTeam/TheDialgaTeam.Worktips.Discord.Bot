using System;
using System.Threading.Tasks;
using TheDialgaTeam.Cryptonote.Rpc.Worktips;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.mscorlib.System.Threading.Tasks;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc
{
    public sealed class RpcService : IInitializable, IBackgroundLoopingTask, IDisposable
    {
        public DaemonRpcClient DaemonRpcClient { get; private set; }

        public WalletRpcClient WalletRpcClient { get; private set; }

        public BackgroundLoopingTask RunningTask { get; private set; }

        private Program Program { get; }

        private LoggerService LoggerService { get; }

        private ConfigService ConfigService { get; }

        private DateTimeOffset? NextCheck { get; set; }

        public RpcService(Program program, LoggerService loggerService, ConfigService configService)
        {
            Program = program;
            LoggerService = loggerService;
            ConfigService = configService;
        }

        public void Initialize()
        {
            DaemonRpcClient = new DaemonRpcClient(ConfigService.DaemonHost, ConfigService.DaemonPort, ConfigService.DaemonUsername, ConfigService.DaemonPassword);
            WalletRpcClient = new WalletRpcClient(ConfigService.WalletHost, ConfigService.WalletPort, ConfigService.WalletUsername, ConfigService.WalletPassword);

            RunningTask = Task.Factory.StartNewBackgroundLoopingTask(async cancellationTokenSource =>
            {
                if (NextCheck == null)
                    NextCheck = DateTimeOffset.Now.AddMinutes(15);

                if (DateTimeOffset.Now < NextCheck)
                    return;

                try
                {
                    LoggerService.LogMessage("Attempting to save the wallet...");
                    await WalletRpcClient.StoreAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                    LoggerService.LogMessage("Save complete!", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    LoggerService.LogErrorMessage(ex);
                }
                finally
                {
                    NextCheck = DateTimeOffset.Now.AddMinutes(15);
                }
            }, Program.CancellationTokenSource.Token);
        }

        public void Dispose()
        {
            DaemonRpcClient?.Dispose();
        }
    }
}