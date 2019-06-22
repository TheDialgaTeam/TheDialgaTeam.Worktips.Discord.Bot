using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheDialgaTeam.Worktips.Discord.Bot.EntityFramework
{
    public class WalletAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint WalletAccountId { get; set; }

        public ulong UserId { get; set; }

        public uint AccountIndex { get; set; }

        public string RegisteredWalletAddress { get; set; }

        public string TipWalletAddress { get; set; }
    }
}