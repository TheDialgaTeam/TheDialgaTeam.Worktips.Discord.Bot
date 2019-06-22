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

        protected async Task<IUserMessage> ReplyAsync(string text)
        {
            try
            {
                if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                    return await Context.Channel.SendMessageAsync(text).ConfigureAwait(false);

                if (GetChannelPermissions().SendMessages)
                    return await Context.Channel.SendMessageAsync(text).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }

            return null;
        }

        protected async Task<IUserMessage> ReplyAsync(Embed embed)
        {
            try
            {
                if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                    return await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                if (GetChannelPermissions().SendMessages)
                    return await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }

            return null;
        }

        protected async Task<IUserMessage> ReplyDMAsync(string text)
        {
            try
            {
                if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                    return await ReplyAsync(text).ConfigureAwait(false);

                var dmChannel = await Context.Message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                return await dmChannel.SendMessageAsync(text).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }

            return null;
        }

        protected async Task<IUserMessage> ReplyDMAsync(Embed embed)
        {
            try
            {
                if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                    return await ReplyAsync(embed).ConfigureAwait(false);

                var dmChannel = await Context.Message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                return await dmChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }

            return null;
        }

        protected async Task AddReactionAsync(string emoji)
        {
            try
            {
                if (Context.Message.Channel is SocketDMChannel || Context.Message.Channel is SocketGroupChannel)
                {
                    await Context.Message.AddReactionAsync(new Emoji(emoji)).ConfigureAwait(false);
                    return;
                }
                
                if (GetChannelPermissions().AddReactions)
                    await Context.Message.AddReactionAsync(new Emoji(emoji)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await CatchError(ex).ConfigureAwait(false);
            }
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

            await ReplyAsync(embedBuilder.Build()).ConfigureAwait(false);

            LoggerService.LogErrorMessage(ex);
        }
    }
}