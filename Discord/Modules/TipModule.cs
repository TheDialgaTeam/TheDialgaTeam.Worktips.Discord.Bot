using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TheDialgaTeam.Worktips.Discord.Bot.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    [Name("Tip")]
    public sealed class TipModule : ModuleHelper
    {
        public TipModule(SqliteDatabaseService sqliteDatabaseService, LoggerService loggerService, RpcService rpcService, ConfigService configService) : base(sqliteDatabaseService, loggerService, rpcService, configService)
        {
        }

        private static bool CheckWalletAddress(string address)
        {
            return address.StartsWith("Wtma", StringComparison.Ordinal) || address.StartsWith("Wtmi", StringComparison.Ordinal) || address.StartsWith("Wtms", StringComparison.Ordinal);
        }

        [Command("RegisterWallet")]
        [Summary("Registers your wallet with the tip bot.")]
        public async Task RegisterWalletAsync([Summary("Wallet address.")] [Remainder]
            string address)
        {
            await DeleteMessageAsync().ConfigureAwait(false);

            if (CheckWalletExist())
            {
                await ReplyAsync("You have already registered an address, use UpdateWallet command if you'd like to update it.").ConfigureAwait(false);
                return;
            }

            if (!CheckWalletAddress(address))
            {
                await ReplyAsync($"Address is not a valid {ConfigService.CoinName} address!").ConfigureAwait(false);
                return;
            }

            try
            {
                var newAccount = await RpcService.WalletRpcClient.CreateAccountAsync().ConfigureAwait(false);

                using (var sqliteContext = SqliteDatabaseService.GetContext())
                {
                    var wallet = new WalletAccount
                    {
                        UserId = Context.User.Id,
                        AccountIndex = newAccount.AccountIndex,
                        RegisteredWalletAddress = address,
                        TipWalletAddress = newAccount.Address
                    };

                    sqliteContext.WalletAccountTable.Add(wallet);
                    await sqliteContext.SaveChangesAsync().ConfigureAwait(false);
                }

                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle("Successfully registered your wallet!")
                    .WithDescription($"Deposit {ConfigService.CoinSymbol} to start tipping!\n\nAddress: **{newAccount.Address}**");

                await ReplyDMAsync("", false, embed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("UpdateWallet")]
        [Summary("Update your registered wallet with the tip bot.")]
        public async Task UpdateWalletAsync([Summary("Wallet address.")] [Remainder]
            string address)
        {
            await DeleteMessageAsync().ConfigureAwait(false);

            if (!CheckWalletExist())
            {
                await RegisterWalletAsync(address).ConfigureAwait(false);
                return;
            }

            if (!CheckWalletAddress(address))
            {
                await ReplyAsync($"Address is not a valid {ConfigService.CoinName} address!").ConfigureAwait(false);
                return;
            }

            using (var sqliteContext = SqliteDatabaseService.GetContext())
            {
                var wallet = sqliteContext.WalletAccountTable.FirstOrDefault(a => a.UserId == Context.User.Id);

                if (wallet != null)
                {
                    wallet.RegisteredWalletAddress = address;

                    sqliteContext.WalletAccountTable.Update(wallet);
                    await sqliteContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            await ReplyDMAsync("Successfully updated your wallet!").ConfigureAwait(false);
        }

        private bool CheckWalletExist()
        {
            using (var sqliteContext = SqliteDatabaseService.GetContext(true))
                return sqliteContext.WalletAccountTable.Count(a => a.UserId == Context.User.Id) > 0;
        }
    }
}