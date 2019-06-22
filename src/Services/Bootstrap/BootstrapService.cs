using System.Reflection;
using System.Runtime.Versioning;
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
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var frameworkVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            System.Console.Title = $"Worktips Discord Bot v{version} {frameworkVersion}";

            LoggerService.LogMessage("==================================================");
            LoggerService.LogMessage($"Worktips Discord Bot v{version} {frameworkVersion}");
            LoggerService.LogMessage("==================================================");
        }
    }
}