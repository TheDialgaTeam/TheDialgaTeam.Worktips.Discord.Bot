using System;
using System.Threading.Tasks;
using Discord.Commands;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    [Name("Daemon")]
    public sealed class DaemonModule : ModuleHelper
    {
        public DaemonModule(SqliteDatabaseService sqliteDatabaseService, LoggerService loggerService, RpcService rpcService, ConfigService configService) : base(sqliteDatabaseService, loggerService, rpcService, configService)
        {
        }

        private static string FormatHashrate(decimal hashrate)
        {
            var i = 0;
            string[] Units = { " H/s", " KH/s", " MH/s", " GH/s", " TH/s", " PH/s" };

            while (hashrate > 1000)
            {
                hashrate /= 1000;
                i++;
            }

            return $"{hashrate:N} {Units[i]}";
        }

        [Command("Hashrate")]
        [Summary("Get the network hashrate.")]
        public async Task HashrateAsync()
        {
            try
            {
                var result = await RpcService.DaemonRpcClient.GetInfoAsync().ConfigureAwait(false);
                var hashrate = Convert.ToDecimal(result.Difficulty) / Convert.ToDecimal(result.Target);

                await ReplyAsync($"The current network hashrate is **{FormatHashrate(hashrate)}**").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("Difficulty")]
        [Summary("Get the network difficulty. (analogous to the strength of the network)")]
        public async Task DifficultyAsync()
        {
            try
            {
                var result = await RpcService.DaemonRpcClient.GetInfoAsync().ConfigureAwait(false);

                await ReplyAsync($"The current difficulty is **{result.Difficulty}**").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("Height")]
        [Summary("Get the current length of longest chain known to daemon.")]
        public async Task HeightAsync()
        {
            try
            {
                var result = await RpcService.DaemonRpcClient.GetInfoAsync().ConfigureAwait(false);

                await ReplyAsync($"The current height is **{result.Height}**").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }
    }
}