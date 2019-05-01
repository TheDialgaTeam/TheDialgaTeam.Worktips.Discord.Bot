using System;
using System.Linq;
using System.Threading.Tasks;
using TheDialgaTeam.Cryptonote.Rpc.Worktips;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.RPC
{
    public sealed class RpcService : IInitializable, IDisposable
    {
        public DaemonRpcClient DaemonRpcClient { get; private set; }

        public WalletRpcClient WalletRpcClient { get; private set; }

        private SettingService SettingService { get; }

        public RpcService(SettingService settingService)
        {
            SettingService = settingService;
        }

        public void Initialize()
        {
            DaemonRpcClient = new DaemonRpcClient(SettingService.DaemonHost, SettingService.DaemonPort, SettingService.DaemonUsername, SettingService.DaemonPassword);
            WalletRpcClient = new WalletRpcClient(SettingService.WalletHost, SettingService.WalletPort, SettingService.WalletUsername, SettingService.WalletPassword);
        }

        public void Dispose()
        {
            DaemonRpcClient?.Dispose();
        }
    }
}