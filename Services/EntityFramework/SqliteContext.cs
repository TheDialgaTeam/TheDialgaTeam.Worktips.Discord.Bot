using Microsoft.EntityFrameworkCore;
using TheDialgaTeam.Worktips.Discord.Bot.Services.IO;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework
{
    public sealed class SqliteContext : DbContext
    {
        private FilePathService FilePathService { get; }

        private bool ReadOnly { get; }

        public SqliteContext()
        {
        }

        public SqliteContext(FilePathService filePathService, bool readOnly = false)
        {
            FilePathService = filePathService;
            ReadOnly = readOnly;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(FilePathService == null ? "Data Source=Application.db" : $"Data Source={FilePathService.SqliteDatabaseFilePath}");

            if (ReadOnly)
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}