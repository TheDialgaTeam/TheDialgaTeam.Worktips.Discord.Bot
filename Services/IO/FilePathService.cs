using System;
using System.IO;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.IO
{
    public sealed class FilePathService : IInitializable
    {
        public string ConsoleLogFilePath { get; private set; }

        public string SqliteDatabaseFilePath { get; private set; }

        public string SettingFilePath { get; private set; }

        public void Initialize()
        {
            var logsDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");
            var dataDirectory = Path.Combine(Environment.CurrentDirectory, "Data");

            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);

            ConsoleLogFilePath = Path.Combine(logsDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

            if (!Directory.Exists(dataDirectory))
                Directory.CreateDirectory(dataDirectory);

            SqliteDatabaseFilePath = Path.Combine(dataDirectory, "Application.db");

            SettingFilePath = Path.Combine(dataDirectory, "Config.json");
        }
    }
}