using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.mscorlib.System.Threading.Tasks;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Console
{
    public sealed class ConsoleCommandService : IInitializable, IBackgroundLoopingTask, IDisposable
    {
        public BackgroundLoopingTask RunningTask { get; private set; }

        private Program Program { get; }

        private LoggerService LoggerService { get; }

        private Dictionary<string, Action> CommandHandlers { get; set; }

        public ConsoleCommandService(Program program, LoggerService loggerService)
        {
            Program = program;
            LoggerService = loggerService;
        }

        public void Initialize()
        {
            CommandHandlers = new Dictionary<string, Action>
            {
                { "exit", ExitCommandHandler }
            };

            LoggerService.LogMessage("All modules loaded. Type \"Help\" to see the help menu. Type \"Exit\" to safely close the application.");

            RunningTask = Task.Factory.StartNewBackgroundLoopingTask(cancellationTokenSource =>
            {
                var input = System.Console.In.ReadLine()?.Trim()?.ToLowerInvariant();

                if (input == null)
                {
                    cancellationTokenSource.Cancel();
                    return;
                }

                if (CommandHandlers.TryGetValue(input, out var commandHandler))
                    commandHandler();
                else
                    LoggerService.LogMessage("Invalid command.", ConsoleColor.Red);
            }, Program.CancellationTokenSource.Token);
        }

        private void ExitCommandHandler()
        {
            Program.CancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            RunningTask?.Dispose();
        }
    }
}