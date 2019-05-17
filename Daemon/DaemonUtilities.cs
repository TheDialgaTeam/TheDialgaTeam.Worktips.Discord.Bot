namespace TheDialgaTeam.Worktips.Discord.Bot.Daemon
{
    public static class DaemonUtilities
    {
        public static string FormatHashrate(decimal hashrate)
        {
            var i = 0;
            string[] units = { "H/s", "KH/s", "MH/s", "GH/s", "TH/s", "PH/s" };

            while (hashrate > 1000)
            {
                hashrate /= 1000;
                i++;
            }

            return $"{hashrate:F2} {units[i]}";
        }
    }
}