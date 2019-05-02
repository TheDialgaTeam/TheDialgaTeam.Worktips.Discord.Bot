using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    public abstract class ModuleHelper : ModuleBase<ShardedCommandContext>
    {
        protected LoggerService LoggerService { get; }

        protected ConfigService ConfigService { get; }

        protected SqliteDatabaseService SqliteDatabaseService { get; }

        protected RpcService RpcService { get; }

        protected ModuleHelper(LoggerService loggerService, ConfigService configService, SqliteDatabaseService sqliteDatabaseService, RpcService rpcService)
        {
            LoggerService = loggerService;
            ConfigService = configService;
            SqliteDatabaseService = sqliteDatabaseService;
            RpcService = rpcService;
        }

        protected override async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                return await Context.Channel.SendMessageAsync(message, isTTS, embed, options).ConfigureAwait(false);

            if (GetChannelPermissions().SendMessages)
                return await Context.Channel.SendMessageAsync(message, isTTS, embed, options).ConfigureAwait(false);

            return null;
        }

        protected override async void AfterExecute(CommandInfo command)
        {
            if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                return;
        }

        protected async Task<IUserMessage> ReplyDMAsync(string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                return await ReplyAsync(text, isTTS, embed, options).ConfigureAwait(false);

            var dmChannel = await Context.Message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(false);
            return await dmChannel.SendMessageAsync(text, isTTS, embed, options).ConfigureAwait(false);
        }

        protected ChannelPermissions GetChannelPermissions(ulong? channelId = null)
        {
            return Context.Guild.GetUser(Context.Client.CurrentUser.Id).GetPermissions(Context.Guild.GetChannel(channelId ?? Context.Channel.Id));
        }

        protected async Task DeleteMessageAsync()
        {
            if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                return;

            if (GetChannelPermissions().ManageMessages)
                await Context.Message.DeleteAsync().ConfigureAwait(false);
        }

        protected async Task CatchError(Exception ex)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Oops, this command resulted in an error:")
                .WithColor(Color.Red)
                .WithDescription($"{ex.Message}")
                .WithFooter("More information have been logged in the bot logger.")
                .WithTimestamp(DateTimeOffset.Now);

            await ReplyAsync("", false, embedBuilder.Build()).ConfigureAwait(false);

            LoggerService.LogErrorMessage(ex);
        }
    }
}