using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TheDialgaTeam.Worktips.Discord.Bot.Discord.Command;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot.Discord.Modules
{
    [Name("Help")]
    public sealed class HelpModule : ModuleHelper
    {
        private IServiceProvider ServiceProvider { get; }

        private CommandService CommandService { get; }

        public HelpModule(SqliteDatabaseService sqliteDatabaseService, LoggerService loggerService, RpcService rpcService, ConfigService configService, Program program) : base(loggerService, configService, sqliteDatabaseService, rpcService)
        {
            ServiceProvider = program.ServiceProvider;
            CommandService = program.CommandService;
        }

        private static bool CheckCommandEquals(CommandInfo command, string commandName)
        {
            if (command.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var commandAlias in command.Aliases)
            {
                if (commandAlias.Equals(commandName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string AppendNotes(CommandInfo commandInfo)
        {
            var stringBuilder = new StringBuilder();
            var ignoredTypes = new List<Type>();

            foreach (var commandInfoParameter in commandInfo.Parameters)
            {
                if (commandInfoParameter.Type.Name == typeof(bool).Name && !ignoredTypes.Contains(typeof(bool)))
                {
                    stringBuilder.AppendLine($"{typeof(bool).Name} arguments can be true or false.\n");
                    ignoredTypes.Add(typeof(bool));
                }
                else if (commandInfoParameter.Type.Name == typeof(char).Name && !ignoredTypes.Contains(typeof(char)))
                {
                    stringBuilder.AppendLine($"{typeof(char).Name} arguments only accepts one single character without quotes.\n");
                    ignoredTypes.Add(typeof(char));
                }
                else if (commandInfoParameter.Type.Name == typeof(sbyte).Name && !ignoredTypes.Contains(typeof(sbyte)))
                {
                    stringBuilder.AppendLine($"{typeof(sbyte).Name} arguments only accepts {sbyte.MinValue} to {sbyte.MaxValue}.\n");
                    ignoredTypes.Add(typeof(sbyte));
                }
                else if (commandInfoParameter.Type.Name == typeof(byte).Name && !ignoredTypes.Contains(typeof(byte)))
                {
                    stringBuilder.AppendLine($"{typeof(byte).Name} arguments only accepts {byte.MinValue} to {byte.MaxValue}.\n");
                    ignoredTypes.Add(typeof(byte));
                }
                else if (commandInfoParameter.Type.Name == typeof(ushort).Name && !ignoredTypes.Contains(typeof(ushort)))
                {
                    stringBuilder.AppendLine($"{typeof(ushort).Name} arguments only accepts {ushort.MinValue} to {ushort.MaxValue}.\n");
                    ignoredTypes.Add(typeof(ushort));
                }
                else if (commandInfoParameter.Type.Name == typeof(short).Name && !ignoredTypes.Contains(typeof(short)))
                {
                    stringBuilder.AppendLine($"{typeof(short).Name} arguments only accepts {short.MinValue} to {short.MaxValue}.\n");
                    ignoredTypes.Add(typeof(short));
                }
                else if (commandInfoParameter.Type.Name == typeof(uint).Name && !ignoredTypes.Contains(typeof(uint)))
                {
                    stringBuilder.AppendLine($"{typeof(uint).Name} arguments only accepts {uint.MinValue} to {uint.MaxValue}.\n");
                    ignoredTypes.Add(typeof(uint));
                }
                else if (commandInfoParameter.Type.Name == typeof(int).Name && !ignoredTypes.Contains(typeof(int)))
                {
                    stringBuilder.AppendLine($"{typeof(int).Name} arguments only accepts {int.MinValue} to {int.MaxValue}.\n");
                    ignoredTypes.Add(typeof(int));
                }
                else if (commandInfoParameter.Type.Name == typeof(ulong).Name && !ignoredTypes.Contains(typeof(ulong)))
                {
                    stringBuilder.AppendLine($"{typeof(ulong).Name} arguments only accepts {ulong.MinValue} to {ulong.MaxValue}.\n");
                    ignoredTypes.Add(typeof(ulong));
                }
                else if (commandInfoParameter.Type.Name == typeof(long).Name && !ignoredTypes.Contains(typeof(long)))
                {
                    stringBuilder.AppendLine($"{typeof(long).Name} arguments only accepts {long.MinValue} to {long.MaxValue}.\n");
                    ignoredTypes.Add(typeof(long));
                }
                else if (commandInfoParameter.Type.Name == typeof(float).Name && !ignoredTypes.Contains(typeof(float)))
                {
                    stringBuilder.AppendLine($"{typeof(float).Name} arguments only accepts {float.MinValue} to {float.MaxValue}.\n");
                    ignoredTypes.Add(typeof(float));
                }
                else if (commandInfoParameter.Type.Name == typeof(double).Name && !ignoredTypes.Contains(typeof(double)))
                {
                    stringBuilder.AppendLine($"{typeof(double).Name} arguments only accepts {double.MinValue} to {double.MaxValue}.\n");
                    ignoredTypes.Add(typeof(double));
                }
                else if (commandInfoParameter.Type.Name == typeof(decimal).Name && !ignoredTypes.Contains(typeof(decimal)))
                {
                    stringBuilder.AppendLine($"{typeof(decimal).Name} arguments only accepts {decimal.MinValue} to {decimal.MaxValue}.\n");
                    ignoredTypes.Add(typeof(decimal));
                }
                else if (commandInfoParameter.Type.Name == typeof(string).Name && !ignoredTypes.Contains(typeof(string)))
                {
                    stringBuilder.AppendLine($"{typeof(string).Name} arguments must be double quoted except for the remainder string type.\n");
                    ignoredTypes.Add(typeof(string));
                }
                else if (commandInfoParameter.Type.Name == typeof(TimeSpan).Name && !ignoredTypes.Contains(typeof(TimeSpan)))
                {
                    stringBuilder.AppendLine($@"{typeof(TimeSpan).Name} arguments must be in one of these format:
`#d#h#m#s`, `#d#h#m`, `#d#h#s`, `#d#h`, `#d#m#s`, `#d#m`, `#d#s`, `#d`, `#h#m#s`, `#h#m`, `#h#s`, `#h`, `#m#s`, `#m`, `#s`

#: Number of units. (d,h,m,s)
d: Days, ranging from 0 to 10675199.
h: Hours, ranging from 0 to 23.
m: Minutes, ranging from 0 to 59.
s: Optional seconds, ranging from 0 to 59.");
                    ignoredTypes.Add(typeof(TimeSpan));
                }
                else if (commandInfoParameter.Type.Name == typeof(IChannel).Name && !ignoredTypes.Contains(typeof(IChannel)))
                {
                    stringBuilder.AppendLine($"{typeof(IChannel).Name} arguments can be #channel, channel id, channel name of any scope.\n");
                    ignoredTypes.Add(typeof(IChannel));
                }
                else if (commandInfoParameter.Type.Name == typeof(IUser).Name && !ignoredTypes.Contains(typeof(IUser)))
                {
                    stringBuilder.AppendLine($"{typeof(IUser).Name} arguments can be @user, user id, username, nickname of any scope.\n");
                    ignoredTypes.Add(typeof(IUser));
                }
                else if (commandInfoParameter.Type.Name == typeof(IRole).Name && !ignoredTypes.Contains(typeof(IRole)))
                {
                    stringBuilder.AppendLine($"{typeof(IRole).Name} arguments can be @role, role id, role name.\n");
                    ignoredTypes.Add(typeof(IRole));
                }
                else if (commandInfoParameter.Type.Name == typeof(IEmote).Name && !ignoredTypes.Contains(typeof(IEmote)))
                {
                    stringBuilder.AppendLine($"{typeof(IEmote).Name} arguments is discord emojis.\n");
                    ignoredTypes.Add(typeof(IEmote));
                }
            }

            return stringBuilder.ToString();
        }

        [Command("Help")]
        public async Task HelpAsync()
        {
            var helpMessage = new EmbedBuilder()
                .WithTitle("Available Command:")
                .WithColor(Color.Orange)
                .WithDescription($"To find out more about each command, use `@{Context.Client.CurrentUser} help <CommandName>`\nIn DM, you can use `help <CommandName>`");

            foreach (var module in CommandService.Modules)
            {
                var moduleName = $"{module.Name} Module";

                if (moduleName == "Help Module")
                    continue;

                var commandInfo = new StringBuilder();

                foreach (var command in module.Commands)
                {
                    var preconditionResult = await command.CheckPreconditionsAsync(Context, ServiceProvider);

                    if (!preconditionResult.IsSuccess)
                        continue;

                    commandInfo.Append($"`{command.Name}`");

                    if (command.Aliases.Count > 0)
                    {
                        foreach (var commandAlias in command.Aliases)
                        {
                            if (!commandAlias.Equals(command.Name, StringComparison.OrdinalIgnoreCase))
                                commandInfo.Append($" `{commandAlias}`");
                        }
                    }

                    commandInfo.AppendLine($": {command.Summary}");
                }

                if (commandInfo.Length > 0)
                    helpMessage = helpMessage.AddField(moduleName, commandInfo.ToString());
            }

            await ReplyAsync("", false, helpMessage.Build()).ConfigureAwait(false);
        }

        [Command("Help")]
        public async Task HelpAsync([Remainder] string commandName)
        {
            foreach (var commandServiceModule in CommandService.Modules)
            {
                var moduleName = $"{commandServiceModule.Name} Module";

                if (moduleName == "Help Module")
                    continue;

                foreach (var command in commandServiceModule.Commands)
                {
                    if (!CheckCommandEquals(command, commandName))
                        continue;

                    var helpMessage = new EmbedBuilder()
                        .WithTitle("Command Info:")
                        .WithColor(Color.Orange)
                        .WithDescription($"To find out more about each command, use `@{Context.Client.CurrentUser} help <CommandName>`\nIn DM, you can use `help <CommandName>`");

                    var requiredPermission = RequiredPermission.GuildMember;
                    var requiredContext = ContextType.Guild | ContextType.DM | ContextType.Group;

                    foreach (var commandAttribute in command.Preconditions)
                    {
                        switch (commandAttribute)
                        {
                            case RequirePermissionAttribute requirePermissionAttribute:
                                requiredPermission = requirePermissionAttribute.RequiredPermission;
                                break;

                            case RequireContextAttribute requireContextAttribute:
                                requiredContext = requireContextAttribute.Contexts;
                                break;
                        }
                    }

                    var requiredContexts = new List<string>();

                    if ((requiredContext & ContextType.Guild) == ContextType.Guild)
                        requiredContexts.Add(ContextType.Guild.ToString());

                    if ((requiredContext & ContextType.DM) == ContextType.DM)
                        requiredContexts.Add(ContextType.DM.ToString());

                    if ((requiredContext & ContextType.Group) == ContextType.Group)
                        requiredContexts.Add(ContextType.Group.ToString());

                    var requiredContextString = string.Join(", ", requiredContexts);

                    var commandInfo = new StringBuilder($"Usage: {Context.Client.CurrentUser.Mention} {command.Name}");
                    var argsInfo = new StringBuilder();

                    foreach (var commandParameter in command.Parameters)
                    {
                        if (commandParameter.IsMultiple)
                            commandInfo.Append($" `params {commandParameter.Type.Name}[] {commandParameter.Name}`");
                        else if (commandParameter.IsOptional)
                            commandInfo.Append($" `{(commandParameter.IsRemainder ? "Remainder " : "")}{commandParameter.Type.Name} {commandParameter.Name} = {commandParameter.DefaultValue ?? "null"}`");
                        else
                            commandInfo.Append($" `{(commandParameter.IsRemainder ? "Remainder " : "")}{commandParameter.Type.Name} {commandParameter.Name}`");

                        argsInfo.AppendLine($"{commandParameter.Type.Name} {commandParameter.Name}: {commandParameter.Summary}");
                    }

                    commandInfo.AppendLine($"\nDescription: {command.Summary}");
                    commandInfo.AppendLine($"Required Permission: {requiredPermission.ToString()}");
                    commandInfo.AppendLine($"Required Context: {requiredContextString}");

                    if (argsInfo.Length > 0)
                    {
                        commandInfo.AppendLine("\nArguments Info:");
                        commandInfo.Append(argsInfo);
                    }

                    if (!string.IsNullOrEmpty(AppendNotes(command)))
                    {
                        commandInfo.AppendLine("\nNote:");
                        commandInfo.Append(AppendNotes(command));
                    }

                    helpMessage = helpMessage.AddField($"{command.Name} command:", commandInfo.ToString());

                    await ReplyAsync("", false, helpMessage.Build()).ConfigureAwait(false);
                    return;
                }
            }
        }
    }
}