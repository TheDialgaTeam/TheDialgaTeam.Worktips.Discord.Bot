namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Setting
{
    public sealed class Config
    {
        public string BotToken { get; set; } = "";

        public string BotPrefix { get; set; } = "/";

        public string CoinName { get; set; } = "Worktips";

        public string CoinSymbol { get; set; } = "WTIP";

        public ulong CoinUnit { get; set; } = 100000000;

        public ulong TipFee { get; set; } = 50000000;

        public ulong TipMixIn { get; set; } = 10;

        public string DaemonHost { get; set; } = "127.0.0.1";

        public ushort DaemonPort { get; set; } = 31022;

        public string DaemonUsername { get; set; } = null;

        public string DaemonPassword { get; set; } = null;

        public string WalletHost { get; set; } = "127.0.0.1";

        public ushort WalletPort { get; set; } = 31024;

        public string WalletUsername { get; set; } = null;

        public string WalletPassword { get; set; } = null;
    }
}