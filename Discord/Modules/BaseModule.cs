using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.RPC;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    [Name("Base")]
    public sealed class BaseModule : ModuleHelper
    {
        public BaseModule(SqliteDatabaseService sqliteDatabaseService, LoggerService loggerService, RpcService rpcService, SettingService settingService) : base(sqliteDatabaseService, loggerService, rpcService, settingService)
        {
        }

        [Command("Ping")]
        [Summary("Gets the estimated round-trip latency, in milliseconds, to the gateway server.")]
        public async Task PingAsync()
        {
            await ReplyAsync($"Ping: {Context.Client.Latency} ms").ConfigureAwait(false);
        }

        [Command("About")]
        [Summary("Get the bot information.")]
        public async Task AboutAsync()
        {
            var applicationInfo = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            var helpMessage = new EmbedBuilder()
                .WithTitle("Worktips Discord Bot:")
                .WithColor(Color.Orange)
                .WithDescription($@"Hello, I am **{Context.Client.CurrentUser.Username}**, a multipurpose worktips bot that is created by jianmingyong#4964.

I am owned by **{applicationInfo.Owner}**.

Type `@{Context.Client.CurrentUser} help` to see my command. You can also type `help` in this DM to see any command that can be used in this DM.

You can invite this bot by using this link: <https://discordapp.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=0&scope=bot>");

            await ReplyDMAsync("", false, helpMessage.Build()).ConfigureAwait(false);
        }
    }
}