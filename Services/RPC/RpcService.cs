using System;
using System.Threading.Tasks;
using TheDialgaTeam.Cryptonote.Rpc.Worktips;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.mscorlib.System.Threading.Tasks;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.RPC
{
    public sealed class RpcService : IInitializable, IBackgroundLoopingTask, IDisposable
    {
        public DaemonRpcClient DaemonRpcClient { get; private set; }

        public bool IsDaemonOnline { get; private set; }

        public BackgroundLoopingTask RunningTask { get; private set; }

        public DateTimeOffset? NextCheck { get; private set; }

        private SettingService SettingService { get; }

        private LoggerService LoggerService { get; }

        private Program Program { get; }

        public RpcService(SettingService settingService, LoggerService loggerService, Program program)
        {
            SettingService = settingService;
            LoggerService = loggerService;
            Program = program;
        }

        public void Initialize()
        {
            DaemonRpcClient = new DaemonRpcClient(SettingService.DaemonHost, SettingService.DaemonPort, SettingService.DaemonUsername, SettingService.DaemonPassword);

            LoggerService.LogMessage("Checking Daemon Status...");
            CheckDaemonStatus().GetAwaiter().GetResult();

            RunningTask = Task.Factory.StartNewBackgroundLoopingTask(async _ =>
            {
                if (NextCheck == null)
                    NextCheck = DateTimeOffset.Now.AddMinutes(15);

                if (DateTimeOffset.Now < NextCheck)
                    return;

                await CheckDaemonStatus().ConfigureAwait(false);

                NextCheck = DateTimeOffset.Now.AddMinutes(15);
            }, Program.CancellationTokenSource.Token);
        }

        private async Task CheckDaemonStatus()
        {
            try
            {
                var daemonInfo = await DaemonRpcClient.GetInfoAsync().ConfigureAwait(false);

                if (daemonInfo.Offline)
                {
                    if (IsDaemonOnline)
                        LoggerService.LogMessage("Daemon is offline!", ConsoleColor.Red);

                    IsDaemonOnline = false;
                }
                else
                {
                    if (!IsDaemonOnline)
                        LoggerService.LogMessage("Daemon is online!", ConsoleColor.Green);

                    IsDaemonOnline = true;
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogErrorMessage(ex);
            }
        }

        public void Dispose()
        {
            DaemonRpcClient?.Dispose();
        }
    }
}