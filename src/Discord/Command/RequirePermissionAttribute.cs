using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Command
{
    public enum RequiredPermission
    {
        DiscordAppOwner = 4,

        GuildAdministrator = 3,

        GuildMember = 0
    }

    public sealed class RequirePermissionAttribute : PreconditionAttribute
    {
        public RequiredPermission RequiredPermission { get; }

        public RequirePermissionAttribute(RequiredPermission requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var currentUserPermission = RequiredPermission.GuildMember;

            if (context.Message.Channel is SocketGuildChannel)
            {
                var guildUser = await context.Guild.GetUserAsync(context.Message.Author.Id).ConfigureAwait(false);

                // Guild Administrator
                if (guildUser.GuildPermissions.Administrator)
                    currentUserPermission = RequiredPermission.GuildAdministrator;
            }

            // Discord App Owner
            var botOwner = (await context.Client.GetApplicationInfoAsync()).Owner;

            if (context.Message.Author.Id == botOwner.Id)
                currentUserPermission = RequiredPermission.DiscordAppOwner;

            return currentUserPermission >= RequiredPermission ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"This command require {RequiredPermission.ToString()} permission and above.");
        }
    }
}