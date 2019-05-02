using System;
using System.IO;
using Newtonsoft.Json;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.IO;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Setting
{
    public sealed class ConfigService : IInitializable, IDisposable
    {
        public string BotToken => Config.BotToken;

        public string BotPrefix => Config.BotPrefix;

        public string CoinName => Config.CoinName;

        public string CoinSymbol => Config.CoinSymbol;

        public decimal CoinUnit => Config.CoinUnit;

        public ulong TipFee => Config.TipFee;

        public ulong TipMixIn => Config.TipMixIn;

        public string DaemonHost => Config.DaemonHost;

        public ushort DaemonPort => Config.DaemonPort;

        public string DaemonUsername => Config.DaemonUsername;

        public string DaemonPassword => Config.DaemonPassword;

        public string WalletHost => Config.WalletHost;

        public ushort WalletPort => Config.WalletPort;

        public string WalletUsername => Config.WalletUsername;

        public string WalletPassword => Config.WalletPassword;

        private Program Program { get; }

        private FilePathService FilePathService { get; }

        private LoggerService LoggerService { get; }

        private Config Config { get; set; }

        public ConfigService(Program program, FilePathService filePathService, LoggerService loggerService)
        {
            Program = program;
            FilePathService = filePathService;
            LoggerService = loggerService;
            Config = new Config();
        }

        public void Initialize()
        {
            if (!File.Exists(FilePathService.SettingFilePath))
            {
                try
                {
                    using (var streamWriter = new StreamWriter(new FileStream(FilePathService.SettingFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                    {
                        var jsonSerializer = new JsonSerializer { Formatting = Formatting.Indented };

                        jsonSerializer.Serialize(streamWriter, Config);
                    }

                    LoggerService.LogMessage($"Generated Configuration file at: {FilePathService.SettingFilePath}");
                }
                catch (Exception ex)
                {
                    LoggerService.LogErrorMessage(ex);
                }
                finally
                {
                    Program.CancellationTokenSource.Cancel();
                }
            }
            else
            {
                try
                {
                    using (var streamReader = new StreamReader(new FileStream(FilePathService.SettingFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        var jsonSerializer = new JsonSerializer();
                        Config = jsonSerializer.Deserialize<Config>(new JsonTextReader(streamReader));
                    }

                    LoggerService.LogMessage("Config loaded!");
                }
                catch (Exception ex)
                {
                    LoggerService.LogErrorMessage(ex);
                    Program.CancellationTokenSource.Cancel();
                }
            }
        }

        public void Dispose()
        {
            try
            {
                using (var streamWriter = new StreamWriter(new FileStream(FilePathService.SettingFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    var jsonSerializer = new JsonSerializer { Formatting = Formatting.Indented };

                    jsonSerializer.Serialize(streamWriter, Config);
                }

                LoggerService.LogMessage($"Saving Configuration file at: {FilePathService.SettingFilePath}");
            }
            catch (Exception ex)
            {
                LoggerService.LogErrorMessage(ex);
            }
        }
    }
}