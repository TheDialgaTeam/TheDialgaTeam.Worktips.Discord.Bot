using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord
{
    public sealed class DiscordAppClient : IDisposable
    {
        public event Func<DiscordAppClient, SocketChannel, Task> ChannelCreated;

        public event Func<DiscordAppClient, SocketChannel, Task> ChannelDestroyed;

        public event Func<DiscordAppClient, SocketChannel, SocketChannel, Task> ChannelUpdated;

        public event Func<DiscordAppClient, SocketSelfUser, SocketSelfUser, Task> CurrentUserUpdated;

        public event Func<DiscordAppClient, SocketGuild, Task> GuildAvailable;

        public event Func<DiscordAppClient, SocketGuild, Task> GuildMembersDownloaded;

        public event Func<DiscordAppClient, SocketGuildUser, SocketGuildUser, Task> GuildMemberUpdated;

        public event Func<DiscordAppClient, SocketGuild, Task> GuildUnavailable;

        public event Func<DiscordAppClient, SocketGuild, SocketGuild, Task> GuildUpdated;

        public event Func<DiscordAppClient, SocketGuild, Task> JoinedGuild;

        public event Func<DiscordAppClient, SocketGuild, Task> LeftGuild;

        public event Func<DiscordAppClient, LogMessage, Task> Log;

        public event Func<DiscordAppClient, Task> LoggedIn;

        public event Func<DiscordAppClient, Task> LoggedOut;

        public event Func<DiscordAppClient, Cacheable<IMessage, ulong>, ISocketMessageChannel, Task> MessageDeleted;

        public event Func<DiscordAppClient, SocketMessage, Task> MessageReceived;

        public event Func<DiscordAppClient, Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task> MessageUpdated;

        public event Func<DiscordAppClient, Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded;

        public event Func<DiscordAppClient, Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved;

        public event Func<DiscordAppClient, Cacheable<IUserMessage, ulong>, ISocketMessageChannel, Task> ReactionsCleared;

        public event Func<DiscordAppClient, SocketGroupUser, Task> RecipientAdded;

        public event Func<DiscordAppClient, SocketGroupUser, Task> RecipientRemoved;

        public event Func<DiscordAppClient, SocketRole, Task> RoleCreated;

        public event Func<DiscordAppClient, SocketRole, Task> RoleDeleted;

        public event Func<DiscordAppClient, SocketRole, SocketRole, Task> RoleUpdated;

        public event Func<DiscordAppClient, DiscordSocketClient, Task> ShardConnected;

        public event Func<DiscordAppClient, Exception, DiscordSocketClient, Task> ShardDisconnected;

        public event Func<DiscordAppClient, int, int, DiscordSocketClient, Task> ShardLatencyUpdated;

        public event Func<DiscordAppClient, DiscordSocketClient, Task> ShardReady;

        public event Func<DiscordAppClient, SocketUser, SocketGuild, Task> UserBanned;

        public event Func<DiscordAppClient, SocketUser, ISocketMessageChannel, Task> UserIsTyping;

        public event Func<DiscordAppClient, SocketGuildUser, Task> UserJoined;

        public event Func<DiscordAppClient, SocketGuildUser, Task> UserLeft;

        public event Func<DiscordAppClient, SocketUser, SocketGuild, Task> UserUnbanned;

        public event Func<DiscordAppClient, SocketUser, SocketUser, Task> UserUpdated;

        public event Func<DiscordAppClient, SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated;

        public event Func<DiscordAppClient, SocketVoiceServer, Task> VoiceServerUpdated;

        public DiscordShardedClient DiscordShardedClient { get; }

        public bool IsLoggedIn { get; private set; }

        public bool IsStarted { get; private set; }

        public DateTimeOffset? NextCheck { get; private set; }

        private string BotToken { get; }

        public DiscordAppClient(string botToken, DiscordSocketConfig config = null)
        {
            BotToken = botToken;
            DiscordShardedClient = new DiscordShardedClient(config ?? new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });

            AddListener();
        }

        public async Task DiscordAppLoginAsync()
        {
            try
            {
                await DiscordShardedClient.LoginAsync(TokenType.Bot, BotToken).ConfigureAwait(false);
            }
            finally
            {
                IsLoggedIn = true;
            }
        }

        public async Task DiscordAppLogoutAsync()
        {
            try
            {
                await DiscordShardedClient.LogoutAsync().ConfigureAwait(false);
            }
            finally
            {
                IsLoggedIn = false;
            }
        }

        public async Task DiscordAppStartAsync()
        {
            try
            {
                await DiscordShardedClient.StartAsync().ConfigureAwait(false);
            }
            finally
            {
                IsStarted = true;
            }
        }

        public async Task DiscordAppStopAsync()
        {
            try
            {
                await DiscordShardedClient.StopAsync().ConfigureAwait(false);
            }
            finally
            {
                IsStarted = false;
            }
        }

        public async Task UpdateAsync()
        {
            if (NextCheck == null)
                NextCheck = DateTimeOffset.Now.AddMinutes(15);

            if (DateTimeOffset.Now < NextCheck)
                return;

            await CheckLoggedInAsync().ConfigureAwait(false);
            await CheckConnectedAsync().ConfigureAwait(false);

            NextCheck = DateTimeOffset.Now.AddMinutes(15);
        }

        private async Task CheckLoggedInAsync()
        {
            try
            {
                if (DiscordShardedClient.LoginState == LoginState.LoggingOut || DiscordShardedClient.LoginState == LoginState.LoggedOut)
                    await DiscordAppLoginAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log(this, new LogMessage(LogSeverity.Error, ToString(), "Unable to authenticate into discord server.", ex)).ConfigureAwait(false);
            }
        }

        private async Task CheckConnectedAsync()
        {
            foreach (var discordSocketClient in DiscordShardedClient.Shards)
            {
                try
                {
                    if (discordSocketClient.ConnectionState == ConnectionState.Disconnected || discordSocketClient.ConnectionState == ConnectionState.Disconnecting)
                        await discordSocketClient.StartAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (Log != null)
                        await Log(this, new LogMessage(LogSeverity.Error, ToString(), "Unable to connect into discord server.", ex)).ConfigureAwait(false);
                }
            }
        }

        private void AddListener()
        {
            DiscordShardedClient.ChannelCreated += DiscordShardedClientOnChannelCreated;
            DiscordShardedClient.ChannelDestroyed += DiscordShardedClientOnChannelDestroyed;
            DiscordShardedClient.ChannelUpdated += DiscordShardedClientOnChannelUpdated;
            DiscordShardedClient.CurrentUserUpdated += DiscordShardedClientOnCurrentUserUpdated;
            DiscordShardedClient.GuildAvailable += DiscordShardedClientOnGuildAvailable;
            DiscordShardedClient.GuildMembersDownloaded += DiscordShardedClientOnGuildMembersDownloaded;
            DiscordShardedClient.GuildMemberUpdated += DiscordShardedClientOnGuildMemberUpdated;
            DiscordShardedClient.GuildUnavailable += DiscordShardedClientOnGuildUnavailable;
            DiscordShardedClient.GuildUpdated += DiscordShardedClientOnGuildUpdated;
            DiscordShardedClient.JoinedGuild += DiscordShardedClientOnJoinedGuild;
            DiscordShardedClient.LeftGuild += DiscordShardedClientOnLeftGuild;
            DiscordShardedClient.Log += DiscordShardedClientOnLog;
            DiscordShardedClient.LoggedIn += DiscordShardedClientOnLoggedIn;
            DiscordShardedClient.LoggedOut += DiscordShardedClientOnLoggedOut;
            DiscordShardedClient.MessageDeleted += DiscordShardedClientOnMessageDeleted;
            DiscordShardedClient.MessageReceived += DiscordShardedClientOnMessageReceived;
            DiscordShardedClient.MessageUpdated += DiscordShardedClientOnMessageUpdated;
            DiscordShardedClient.ReactionAdded += DiscordShardedClientOnReactionAdded;
            DiscordShardedClient.ReactionRemoved += DiscordShardedClientOnReactionRemoved;
            DiscordShardedClient.ReactionsCleared += DiscordShardedClientOnReactionsCleared;
            DiscordShardedClient.RecipientAdded += DiscordShardedClientOnRecipientAdded;
            DiscordShardedClient.RecipientRemoved += DiscordShardedClientOnRecipientRemoved;
            DiscordShardedClient.RoleCreated += DiscordShardedClientOnRoleCreated;
            DiscordShardedClient.RoleDeleted += DiscordShardedClientOnRoleDeleted;
            DiscordShardedClient.RoleUpdated += DiscordShardedClientOnRoleUpdated;
            DiscordShardedClient.ShardConnected += DiscordShardedClientOnShardConnected;
            DiscordShardedClient.ShardDisconnected += DiscordShardedClientOnShardDisconnected;
            DiscordShardedClient.ShardLatencyUpdated += DiscordShardedClientOnShardLatencyUpdated;
            DiscordShardedClient.ShardReady += DiscordShardedClientOnShardReady;
            DiscordShardedClient.UserBanned += DiscordShardedClientOnUserBanned;
            DiscordShardedClient.UserIsTyping += DiscordShardedClientOnUserIsTyping;
            DiscordShardedClient.UserJoined += DiscordShardedClientOnUserJoined;
            DiscordShardedClient.UserLeft += DiscordShardedClientOnUserLeft;
            DiscordShardedClient.UserUnbanned += DiscordShardedClientOnUserUnbanned;
            DiscordShardedClient.UserUpdated += DiscordShardedClientOnUserUpdated;
            DiscordShardedClient.UserVoiceStateUpdated += DiscordShardedClientOnUserVoiceStateUpdated;
            DiscordShardedClient.VoiceServerUpdated += DiscordShardedClientOnVoiceServerUpdated;
        }

        private void RemoveListener()
        {
            DiscordShardedClient.ChannelCreated -= DiscordShardedClientOnChannelCreated;
            DiscordShardedClient.ChannelDestroyed -= DiscordShardedClientOnChannelDestroyed;
            DiscordShardedClient.ChannelUpdated -= DiscordShardedClientOnChannelUpdated;
            DiscordShardedClient.CurrentUserUpdated -= DiscordShardedClientOnCurrentUserUpdated;
            DiscordShardedClient.GuildAvailable -= DiscordShardedClientOnGuildAvailable;
            DiscordShardedClient.GuildMembersDownloaded -= DiscordShardedClientOnGuildMembersDownloaded;
            DiscordShardedClient.GuildMemberUpdated -= DiscordShardedClientOnGuildMemberUpdated;
            DiscordShardedClient.GuildUnavailable -= DiscordShardedClientOnGuildUnavailable;
            DiscordShardedClient.GuildUpdated -= DiscordShardedClientOnGuildUpdated;
            DiscordShardedClient.JoinedGuild -= DiscordShardedClientOnJoinedGuild;
            DiscordShardedClient.LeftGuild -= DiscordShardedClientOnLeftGuild;
            DiscordShardedClient.Log -= DiscordShardedClientOnLog;
            DiscordShardedClient.LoggedIn -= DiscordShardedClientOnLoggedIn;
            DiscordShardedClient.LoggedOut -= DiscordShardedClientOnLoggedOut;
            DiscordShardedClient.MessageDeleted -= DiscordShardedClientOnMessageDeleted;
            DiscordShardedClient.MessageReceived -= DiscordShardedClientOnMessageReceived;
            DiscordShardedClient.MessageUpdated -= DiscordShardedClientOnMessageUpdated;
            DiscordShardedClient.ReactionAdded -= DiscordShardedClientOnReactionAdded;
            DiscordShardedClient.ReactionRemoved -= DiscordShardedClientOnReactionRemoved;
            DiscordShardedClient.ReactionsCleared -= DiscordShardedClientOnReactionsCleared;
            DiscordShardedClient.RecipientAdded -= DiscordShardedClientOnRecipientAdded;
            DiscordShardedClient.RecipientRemoved -= DiscordShardedClientOnRecipientRemoved;
            DiscordShardedClient.RoleCreated -= DiscordShardedClientOnRoleCreated;
            DiscordShardedClient.RoleDeleted -= DiscordShardedClientOnRoleDeleted;
            DiscordShardedClient.RoleUpdated -= DiscordShardedClientOnRoleUpdated;
            DiscordShardedClient.ShardConnected -= DiscordShardedClientOnShardConnected;
            DiscordShardedClient.ShardDisconnected -= DiscordShardedClientOnShardDisconnected;
            DiscordShardedClient.ShardLatencyUpdated -= DiscordShardedClientOnShardLatencyUpdated;
            DiscordShardedClient.ShardReady -= DiscordShardedClientOnShardReady;
            DiscordShardedClient.UserBanned -= DiscordShardedClientOnUserBanned;
            DiscordShardedClient.UserIsTyping -= DiscordShardedClientOnUserIsTyping;
            DiscordShardedClient.UserJoined -= DiscordShardedClientOnUserJoined;
            DiscordShardedClient.UserLeft -= DiscordShardedClientOnUserLeft;
            DiscordShardedClient.UserUnbanned -= DiscordShardedClientOnUserUnbanned;
            DiscordShardedClient.UserUpdated -= DiscordShardedClientOnUserUpdated;
            DiscordShardedClient.UserVoiceStateUpdated -= DiscordShardedClientOnUserVoiceStateUpdated;
            DiscordShardedClient.VoiceServerUpdated -= DiscordShardedClientOnVoiceServerUpdated;
        }

        private async Task DiscordShardedClientOnChannelCreated(SocketChannel arg)
        {
            if (ChannelCreated != null)
                await ChannelCreated(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnChannelDestroyed(SocketChannel arg)
        {
            if (ChannelDestroyed != null)
                await ChannelDestroyed(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnChannelUpdated(SocketChannel arg1, SocketChannel arg2)
        {
            if (ChannelUpdated != null)
                await ChannelUpdated(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnCurrentUserUpdated(SocketSelfUser arg1, SocketSelfUser arg2)
        {
            if (CurrentUserUpdated != null)
                await CurrentUserUpdated(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnGuildAvailable(SocketGuild arg)
        {
            if (GuildAvailable != null)
                await GuildAvailable(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnGuildMembersDownloaded(SocketGuild arg)
        {
            if (GuildMembersDownloaded != null)
                await GuildMembersDownloaded(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            if (GuildMemberUpdated != null)
                await GuildMemberUpdated(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnGuildUnavailable(SocketGuild arg)
        {
            if (GuildUnavailable != null)
                await GuildUnavailable(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnGuildUpdated(SocketGuild arg1, SocketGuild arg2)
        {
            if (GuildUpdated != null)
                await GuildUpdated(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnJoinedGuild(SocketGuild arg)
        {
            if (JoinedGuild != null)
                await JoinedGuild(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnLeftGuild(SocketGuild arg)
        {
            if (LeftGuild != null)
                await LeftGuild(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnLog(LogMessage arg)
        {
            if (Log != null)
                await Log(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnLoggedIn()
        {
            if (LoggedIn != null)
                await LoggedIn(this).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnLoggedOut()
        {
            if (LoggedOut != null)
                await LoggedOut(this).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnMessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if (MessageDeleted != null)
                await MessageDeleted(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnMessageReceived(SocketMessage arg)
        {
            if (MessageReceived != null)
                await MessageReceived(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (MessageUpdated != null)
                await MessageUpdated(this, arg1, arg2, arg3).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (ReactionAdded != null)
                await ReactionAdded(this, arg1, arg2, arg3).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (ReactionRemoved != null)
                await ReactionRemoved(this, arg1, arg2, arg3).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnReactionsCleared(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if (ReactionsCleared != null)
                await ReactionsCleared(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnRecipientAdded(SocketGroupUser arg)
        {
            if (RecipientAdded != null)
                await RecipientAdded(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnRecipientRemoved(SocketGroupUser arg)
        {
            if (RecipientRemoved != null)
                await RecipientRemoved(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnRoleCreated(SocketRole arg)
        {
            if (RoleCreated != null)
                await RoleCreated(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnRoleDeleted(SocketRole arg)
        {
            if (RoleDeleted != null)
                await RoleDeleted(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnRoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            if (RoleUpdated != null)
                await RoleUpdated(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnShardConnected(DiscordSocketClient arg)
        {
            if (ShardConnected != null)
                await ShardConnected(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            if (ShardDisconnected != null)
                await ShardDisconnected(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnShardLatencyUpdated(int arg1, int arg2, DiscordSocketClient arg3)
        {
            if (ShardLatencyUpdated != null)
                await ShardLatencyUpdated(this, arg1, arg2, arg3).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnShardReady(DiscordSocketClient arg)
        {
            if (ShardReady != null)
                await ShardReady(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if (UserBanned != null)
                await UserBanned(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnUserIsTyping(SocketUser arg1, ISocketMessageChannel arg2)
        {
            if (UserIsTyping != null)
                await UserIsTyping(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnUserJoined(SocketGuildUser arg)
        {
            if (UserJoined != null)
                await UserJoined(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnUserLeft(SocketGuildUser arg)
        {
            if (UserLeft != null)
                await UserLeft(this, arg).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnUserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            if (UserUnbanned != null)
                await UserUnbanned(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnUserUpdated(SocketUser arg1, SocketUser arg2)
        {
            if (UserUpdated != null)
                await UserUpdated(this, arg1, arg2).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnUserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (UserVoiceStateUpdated != null)
                await UserVoiceStateUpdated(this, arg1, arg2, arg3).ConfigureAwait(false);
        }

        private async Task DiscordShardedClientOnVoiceServerUpdated(SocketVoiceServer arg)
        {
            if (VoiceServerUpdated != null)
                await VoiceServerUpdated(this, arg).ConfigureAwait(false);
        }

        public void Dispose()
        {
            RemoveListener();
            DiscordShardedClient?.Dispose();
        }
    }
}