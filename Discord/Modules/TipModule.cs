using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TheDialgaTeam.Cryptonote.Rpc.Worktips.Json.Wallet;
using TheDialgaTeam.Worktips.Discord.Bot.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;
using TheDialgaTeam.Worktips.Discord.Bot.Wallet;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    [Name("Tip")]
    public sealed class TipModule : ModuleHelper
    {
        public TipModule(SqliteDatabaseService sqliteDatabaseService, LoggerService loggerService, RpcService rpcService, ConfigService configService) : base(loggerService, configService, sqliteDatabaseService, rpcService)
        {
        }

        [Command("RegisterWallet")]
        [Alias("Register")]
        [Summary("Register/Update your wallet with the tip bot.")]
        public async Task RegisterWalletAsync([Summary("Wallet address.")] [Remainder]
            string address)
        {
            try
            {
                if (!WalletUtilities.CheckWalletAddress(address))
                {
                    await ReplyAsync($"Address is not a valid {ConfigService.CoinName} address!").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                if (WalletUtilities.CheckWalletExist(SqliteDatabaseService, Context.User.Id, out var walletAccount))
                {
                    using (var sqliteContext = SqliteDatabaseService.GetContext())
                    {
                        sqliteContext.WalletAccountTable.Attach(walletAccount);
                        walletAccount.RegisteredWalletAddress = address;
                        sqliteContext.WalletAccountTable.Update(walletAccount);
                        await sqliteContext.SaveChangesAsync().ConfigureAwait(false);
                    }

                    await ReplyDMAsync("Successfully updated your wallet!").ConfigureAwait(false);
                    await AddReactionAsync("✅").ConfigureAwait(false);
                }
                else
                {
                    var newAccount = await RpcService.WalletRpcClient.CreateAccountAsync(Context.User.Id.ToString()).ConfigureAwait(false);

                    if (newAccount == null)
                        throw new Exception("Unable to create account.");

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
                        .WithDescription($"Deposit {ConfigService.CoinSymbol} to start tipping!\n\nAddress: `{newAccount.Address}`");

                    await ReplyDMAsync(embed.Build()).ConfigureAwait(false);
                    await AddReactionAsync("✅").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("WalletInfo")]
        [Alias("Info")]
        [Summary("Display your wallet information with the tip bot.")]
        public async Task WalletInfoAsync([Remainder] string _ = null)
        {
            try
            {
                if (!WalletUtilities.CheckWalletExist(SqliteDatabaseService, Context.User.Id, out var walletAccount))
                {
                    await ReplyAsync("Please use the `RegisterWallet` command to register your wallet first.").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle(":information_desk_person: ACCOUNT INFO")
                    .WithDescription($":purse: Deposit Address: `{walletAccount.TipWalletAddress}`\n\n:purse: Registered Address: `{walletAccount.RegisteredWalletAddress}`");

                await ReplyDMAsync(embed.Build()).ConfigureAwait(false);
                await AddReactionAsync("✅").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("WalletBalance")]
        [Alias("Balance", "Bal")]
        [Summary("Check your wallet balance.")]
        public async Task WalletBalanceAsync([Remainder] string _ = null)
        {
            try
            {
                if (!WalletUtilities.CheckWalletExist(SqliteDatabaseService, Context.User.Id, out var walletAccount))
                {
                    await ReplyAsync("Please use the `RegisterWallet` command to register your wallet first.").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var balance = await RpcService.WalletRpcClient.GetBalanceAsync(walletAccount.AccountIndex).ConfigureAwait(false);
                var walletHeight = await RpcService.WalletRpcClient.GetHeightAsync().ConfigureAwait(false);
                var daemonHeight = await RpcService.DaemonRpcClient.GetInfoAsync().ConfigureAwait(false);

                var availableBalance = balance.UnlockedBalance / Convert.ToDecimal(ConfigService.CoinUnit);
                var pendingBalance = (balance.Balance - balance.UnlockedBalance) / Convert.ToDecimal(ConfigService.CoinUnit);

                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle(":moneybag: YOUR BALANCE")
                    .WithDescription($":moneybag: Available: {WalletUtilities.FormatBalance(ConfigService, availableBalance)} {ConfigService.CoinSymbol}\n:purse: Pending: {WalletUtilities.FormatBalance(ConfigService, pendingBalance)} {ConfigService.CoinSymbol}\n:arrows_counterclockwise: Status: {walletHeight.Height} / {daemonHeight.Height}");

                await ReplyDMAsync(embed.Build()).ConfigureAwait(false);
                await AddReactionAsync("✅").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("BotWalletBalance")]
        [Alias("BotBalance", "BotBal")]
        [Summary("Check the bot wallet balance.")]
        public async Task BotWalletBalanceAsync([Remainder] string _ = null)
        {
            try
            {
                if (!WalletUtilities.CheckWalletExist(SqliteDatabaseService, Context.Client.CurrentUser.Id, out var walletAccount))
                {
                    await ReplyAsync("Please use the `RegisterWallet` command to register your wallet first.").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var balance = await RpcService.WalletRpcClient.GetBalanceAsync(walletAccount.AccountIndex).ConfigureAwait(false);
                var walletHeight = await RpcService.WalletRpcClient.GetHeightAsync().ConfigureAwait(false);
                var daemonHeight = await RpcService.DaemonRpcClient.GetInfoAsync().ConfigureAwait(false);

                var availableBalance = balance.UnlockedBalance / Convert.ToDecimal(ConfigService.CoinUnit);
                var pendingBalance = (balance.Balance - balance.UnlockedBalance) / Convert.ToDecimal(ConfigService.CoinUnit);

                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithTitle(":moneybag: TIP BOT BALANCE")
                    .WithDescription($":moneybag: Available: {WalletUtilities.FormatBalance(ConfigService, availableBalance)} {ConfigService.CoinSymbol}\n:purse: Pending: {WalletUtilities.FormatBalance(ConfigService, pendingBalance)} {ConfigService.CoinSymbol}\n:arrows_counterclockwise: Status: {walletHeight.Height} / {daemonHeight.Height}");

                await ReplyAsync(embed.Build()).ConfigureAwait(false);
                await AddReactionAsync("✅").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("WalletWithdraw")]
        [Alias("Withdraw")]
        [Summary("Withdraw to the registered address.")]
        public async Task WalletWithdrawAsync([Summary("Amount to withdraw")] decimal amount, [Remainder] string _ = null)
        {
            try
            {
                if (!WalletUtilities.CheckWalletExist(SqliteDatabaseService, Context.User.Id, out var walletAccount))
                {
                    await ReplyAsync("Please use the `RegisterWallet` command to register your wallet first.").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var atomicAmountToWithdraw = Convert.ToUInt64(Math.Ceiling(amount * ConfigService.CoinUnit));

                if (atomicAmountToWithdraw < ConfigService.WithdrawMinimumAmount)
                {
                    await ReplyAsync($":x: Minimum withdrawal amount is: {WalletUtilities.FormatBalance(ConfigService, ConfigService.WithdrawMinimumAmount / Convert.ToDecimal(ConfigService.CoinUnit))} {ConfigService.CoinSymbol}").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var balance = await RpcService.WalletRpcClient.GetBalanceAsync(walletAccount.AccountIndex).ConfigureAwait(false);

                if (atomicAmountToWithdraw > balance.UnlockedBalance)
                {
                    await ReplyAsync(":x: Insufficient balance to withdraw this amount.").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var transferRequest = new CommandRpcTransferSplit.Request
                {
                    AccountIndex = walletAccount.AccountIndex,
                    Destinations = new[]
                    {
                        new CommandRpcTransferSplit.TransferDestination
                        {
                            Address = walletAccount.RegisteredWalletAddress,
                            Amount = atomicAmountToWithdraw
                        }
                    },
                    Mixin = ConfigService.TipMixIn,
                    GetTxHex = true
                };

                var transferResult = await RpcService.WalletRpcClient.TransferSplitAsync(transferRequest).ConfigureAwait(false);

                if (!WalletUtilities.IsTransferSuccess(transferResult))
                {
                    var failEmbed = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithTitle(":moneybag: TRANSFER RESULT")
                        .WithDescription("Failed to withdrawn this amount due to insufficient balance to cover the transaction fees.");

                    await ReplyDMAsync(failEmbed.Build()).ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var successEmbed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle(":moneybag: TRANSFER RESULT")
                    .WithDescription($"You have withdrawn {WalletUtilities.FormatBalance(ConfigService, atomicAmountToWithdraw / Convert.ToDecimal(ConfigService.CoinUnit))} {ConfigService.CoinSymbol}");

                await ReplyDMAsync(successEmbed.Build()).ConfigureAwait(false);

                for (var i = 0; i < transferResult.TxHashList.Length; i++)
                {
                    var txAmount = transferResult.AmountList[i] / Convert.ToDecimal(ConfigService.CoinUnit);
                    var txFee = transferResult.FeeList[i] / Convert.ToDecimal(ConfigService.CoinUnit);
                    var txHash = transferResult.TxHashList[i];

                    var txEmbed = new EmbedBuilder()
                        .WithColor(Color.Orange)
                        .WithTitle($":moneybag: TRANSACTION PAID ({i + 1}/{transferResult.TxHashList.Length})")
                        .WithDescription($"Amount: {WalletUtilities.FormatBalance(ConfigService, txAmount)} {ConfigService.CoinSymbol}\nFee: {WalletUtilities.FormatBalance(ConfigService, txFee)} {ConfigService.CoinSymbol}\nTransaction hash: `{txHash}`");

                    await ReplyDMAsync(txEmbed.Build()).ConfigureAwait(false);
                }

                await AddReactionAsync("💰").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }

        [Command("Tip")]
        [Summary("Tip someone using the tip wallet.")]
        [RequireContext(ContextType.Guild)]
        public async Task TipAsync([Summary("Amount to tip")] decimal amount, [Remainder] string _ = null)
        {
            try
            {
                if (!WalletUtilities.CheckWalletExist(SqliteDatabaseService, Context.User.Id, out var walletAccount))
                {
                    await ReplyAsync("Please use the `RegisterWallet` command to register your wallet first.").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var atomicAmountToTip = Convert.ToUInt64(Math.Ceiling(amount * ConfigService.CoinUnit));

                if (atomicAmountToTip < ConfigService.TipMinimumAmount)
                {
                    await ReplyAsync($":x: Minimum tip amount is: {WalletUtilities.FormatBalance(ConfigService, ConfigService.TipMinimumAmount / Convert.ToDecimal(ConfigService.CoinUnit))} {ConfigService.CoinSymbol}").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var balance = await RpcService.WalletRpcClient.GetBalanceAsync(walletAccount.AccountIndex).ConfigureAwait(false);

                if (atomicAmountToTip * Convert.ToUInt64(Context.Message.MentionedUsers.Count) > balance.UnlockedBalance)
                {
                    await ReplyAsync(":x: Insufficient balance to tip this amount.").ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var transferDestinations = new List<CommandRpcTransferSplit.TransferDestination>();
                var userTipped = new List<IUser>();

                foreach (var user in Context.Message.MentionedUsers)
                {
                    // @everyone @here
                    if (user.Id == Context.Guild.Id || user.Id == Context.Channel.Id)
                        continue;

                    if (!WalletUtilities.CheckWalletExist(SqliteDatabaseService, user.Id, out var userWalletAccount))
                        continue;

                    if (user.Id == Context.User.Id)
                        continue;

                    transferDestinations.Add(new CommandRpcTransferSplit.TransferDestination
                    {
                        Address = userWalletAccount.TipWalletAddress,
                        Amount = atomicAmountToTip
                    });

                    userTipped.Add(user);
                }

                if (userTipped.Count == 0)
                {
                    var failEmbed = new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithTitle(":moneybag: TRANSFER RESULT")
                            .WithDescription("Failed to tip this amount due to the users have not registered their wallet.");

                    await ReplyDMAsync(failEmbed.Build()).ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                var transferRequest = new CommandRpcTransferSplit.Request
                {
                    AccountIndex = walletAccount.AccountIndex,
                    Destinations = transferDestinations.ToArray(),
                    Mixin = ConfigService.TipMixIn,
                    GetTxHex = true
                };

                var transferResult = await RpcService.WalletRpcClient.TransferSplitAsync(transferRequest).ConfigureAwait(false);

                if (!WalletUtilities.IsTransferSuccess(transferResult))
                {
                    var failEmbed = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithTitle(":moneybag: TRANSFER RESULT")
                        .WithDescription("Failed to tip this amount due to insufficient balance to cover the transaction fees.");

                    await ReplyDMAsync(failEmbed.Build()).ConfigureAwait(false);
                    await AddReactionAsync("❌").ConfigureAwait(false);
                    return;
                }

                // Tip Success for this user, inform them.
                foreach (var user in userTipped)
                {
                    if (user.IsBot)
                        continue;

                    try
                    {
                        var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);

                        var notificationEmbed = new EmbedBuilder()
                            .WithColor(Color.Green)
                            .WithTitle(":moneybag: INCOMING TIP")
                            .WithDescription($":moneybag: You got a tip of {WalletUtilities.FormatBalance(ConfigService, atomicAmountToTip / Convert.ToDecimal(ConfigService.CoinUnit))} {ConfigService.CoinSymbol} from {Context.User}\n:hash: Transaction hash: {string.Join(", ", transferResult.TxHashList.Select(a => $"`{a}`"))}");

                        await dmChannel.SendMessageAsync("", false, notificationEmbed.Build()).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // ignore.
                    }
                }

                var successEmbed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle(":moneybag: TRANSFER RESULT")
                    .WithDescription($"You have tipped {WalletUtilities.FormatBalance(ConfigService, atomicAmountToTip / Convert.ToDecimal(ConfigService.CoinUnit))} {ConfigService.CoinSymbol} to {userTipped.Count} users");

                await ReplyDMAsync(successEmbed.Build()).ConfigureAwait(false);

                for (var i = 0; i < transferResult.TxHashList.Length; i++)
                {
                    var txAmount = transferResult.AmountList[i] / Convert.ToDecimal(ConfigService.CoinUnit);
                    var txFee = transferResult.FeeList[i] / Convert.ToDecimal(ConfigService.CoinUnit);
                    var txHash = transferResult.TxHashList[i];

                    var txEmbed = new EmbedBuilder()
                        .WithColor(Color.Orange)
                        .WithTitle($":moneybag: TRANSACTION PAID ({i + 1}/{transferResult.TxHashList.Length})")
                        .WithDescription($"Amount: {WalletUtilities.FormatBalance(ConfigService, txAmount)} {ConfigService.CoinSymbol}\nFee: {WalletUtilities.FormatBalance(ConfigService, txFee)} {ConfigService.CoinSymbol}\nTransaction hash: `{txHash}`");

                    await ReplyDMAsync(txEmbed.Build()).ConfigureAwait(false);
                }

                await AddReactionAsync("💰").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
        }
    }
}