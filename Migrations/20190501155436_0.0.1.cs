using Microsoft.EntityFrameworkCore.Migrations;

namespace TheDialgaTeam.Worktips.Discord.Bot.Migrations
{
    public partial class _001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletAccountTable",
                columns: table => new
                {
                    WalletAccountId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(nullable: false),
                    AccountIndex = table.Column<ulong>(nullable: false),
                    RegisteredWalletAddress = table.Column<string>(nullable: true),
                    TipWalletAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletAccountTable", x => x.WalletAccountId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletAccountTable");
        }
    }
}
