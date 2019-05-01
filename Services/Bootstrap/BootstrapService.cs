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
            System.Console.Title = "The Dialga Team Worktips Discord Bot (.Net Core)";

            LoggerService.LogMessage("==================================================");
            LoggerService.LogMessage("The Dialga Team Worktips Discord Bot (.NET Core)");
            LoggerService.LogMessage("==================================================");
        }
    }
}