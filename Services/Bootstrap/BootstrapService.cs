using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Bootstrap
{
    public sealed class BootstrapService : IInitializable
    {
        private LoggerService LoggerService { get; }

        public BootstrapService(LoggerService loggerService)
        {
            LoggerService = loggerService;
        }

        public void Initialize()
        {
            System.Console.Title = "Worktips Discord Bot (.Net Core)";

            LoggerService.LogMessage("==================================================");
            LoggerService.LogMessage("Worktips Discord Bot (.NET Core)");
            LoggerService.LogMessage("==================================================");
        }
    }
}