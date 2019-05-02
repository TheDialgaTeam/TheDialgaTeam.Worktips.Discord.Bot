using System;
using System.Linq;
using TheDialgaTeam.Cryptonote.Rpc.Worktips.Json.Wallet;
using TheDialgaTeam.Worktips.Discord.Bot.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Wallet
{
    public static class WalletUtilities
    {
        public static bool CheckWalletAddress(string address)
        {
            return address.StartsWith("Wtma", StringComparison.Ordinal) || address.StartsWith("Wtmi", StringComparison.Ordinal) || address.StartsWith("Wtms", StringComparison.Ordinal);
        }

        public static bool CheckWalletExist(SqliteDatabaseService sqliteDatabaseService, ulong userId, out WalletAccount walletAccount)
        {
            using (var context = sqliteDatabaseService.GetContext(true))
            {
                walletAccount = context.WalletAccountTable.FirstOrDefault(a => a.UserId == userId);
                return walletAccount != default;
            }
        }

        public static string FormatBalance(ConfigService configService, decimal balance)
        {
            var atomicUnit = configService.CoinUnit;

            int RecursiveDecimalPlaces(ulong value)
            {
                if (value == 1 || value % 10 > 0)
                    return 0;

                return 1 + RecursiveDecimalPlaces(value / 10);
            }

            return balance.ToString("F" + RecursiveDecimalPlaces(atomicUnit));
        }

        public static bool IsTransferSuccess(CommandRpcTransferSplit.Response response)
        {
            return response?.AmountList?.Length > 0;
        }

        public static decimal TotalAtomicAmountSent(CommandRpcTransferSplit.Response response)
        {
            var result = 0ul;

            foreach (var amount in response.AmountList)
                result += amount;

            return result;
        }
    }
}