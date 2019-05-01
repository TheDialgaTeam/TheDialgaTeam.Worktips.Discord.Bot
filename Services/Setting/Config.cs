namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Setting
{
    public sealed class Config
    {
        public string BotToken { get; set; } = "";

        public string BotPrefix { get; set; } = "/";

        public string DaemonHost { get; set; } = "127.0.0.1";

        public ushort DaemonPort { get; set; } = 31022;

        public string DaemonUsername { get; set; } = null;

        public string DaemonPassword { get; set; } = null;

        public string WalletHost { get; set; } = "127.0.0.1";

        public ushort WalletPort { get; set; } = 31023;

        public string WalletUsername { get; set; } = null;

        public string WalletPassword { get; set; } = null;
    }
}