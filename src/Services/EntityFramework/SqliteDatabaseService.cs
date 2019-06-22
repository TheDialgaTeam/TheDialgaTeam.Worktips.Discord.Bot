using System;
using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.IO;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework
{
    public sealed class SqliteDatabaseService : IInitializable
    {
        private FilePathService FilePathService { get; }

        private LoggerService LoggerService { get; }

        public SqliteDatabaseService(FilePathService filePathService, LoggerService loggerService)
        {
            FilePathService = filePathService;
            LoggerService = loggerService;
        }

        public void Initialize()
        {
            LoggerService.LogMessage("Checking Database for updates...");

            using (var context = GetContext(true))
                context.Database.Migrate();

            LoggerService.LogMessage("Database initialized!", ConsoleColor.Green);
        }

        public SqliteContext GetContext(bool readOnly = false)
        {
            return new SqliteContext(FilePathService, readOnly);
        }
    }
}