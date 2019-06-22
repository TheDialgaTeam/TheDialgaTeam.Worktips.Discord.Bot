using System;
using System.Threading.Tasks;
using Discord.Commands;
using TheDialgaTeam.Worktips.Discord.Bot.Daemon;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    [Name("Daemon")]
    public sealed class DaemonModule : ModuleHelper
    {
        public DaemonModule(LoggerService loggerService, ConfigService configService, SqliteDatabaseService sqliteDatabaseService, RpcService rpcService) : base(loggerService, configService, sqliteDatabaseService, rpcService)
        {
        }

        [Command("Hashrate")]
        [Alias("Hash")]
        [Summary("Get the network hashrate.")]
        public async Task HashrateAsync([Remainder] string _ = null)
        {
            try
            {
                var result = await RpcService.DaemonRpcClient.GetInfoAsync().ConfigureAwait(false);
                var hashrate = Convert.ToDecimal(result.Difficulty) / Convert.ToDecimal(result.Target);

                await ReplyAsync($"The current network hashrate is **{DaemonUtilities.FormatHashrate(hashrate)}**").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("Difficulty")]
        [Alias("Diff")]
        [Summary("Get the network difficulty. (analogous to the strength of the network)")]
        public async Task DifficultyAsync([Remainder] string _ = null)
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
        public async Task HeightAsync([Remainder] string _ = null)
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