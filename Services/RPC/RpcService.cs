using System;
using TheDialgaTeam.Cryptonote.Rpc.Worktips;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc
{
    public sealed class RpcService : IInitializable, IDisposable
    {
        public DaemonRpcClient DaemonRpcClient { get; private set; }

        public WalletRpcClient WalletRpcClient { get; private set; }

        private ConfigService ConfigService { get; }

        public RpcService(ConfigService configService)
        {
            ConfigService = configService;
        }

        public void Initialize()
        {
            DaemonRpcClient = new DaemonRpcClient(ConfigService.DaemonHost, ConfigService.DaemonPort, ConfigService.DaemonUsername, ConfigService.DaemonPassword);
            WalletRpcClient = new WalletRpcClient(ConfigService.WalletHost, ConfigService.WalletPort, ConfigService.WalletUsername, ConfigService.WalletPassword);
        }

        public void Dispose()
        {
            DaemonRpcClient?.Dispose();
        }
    }
}