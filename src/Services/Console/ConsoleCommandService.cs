using System;
using System.Threading.Tasks;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Console
{
    public sealed class ConsoleCommandService : IInitializable
    {
        private Program Program { get; }

        private LoggerService LoggerService { get; }

        public ConsoleCommandService(Program program, LoggerService loggerService)
        {
            Program = program;
            LoggerService = loggerService;
        }

        public void Initialize()
        {
            LoggerService.LogMessage("All modules loaded. Type \"Exit\" to safely close the application.");

            System.Console.CancelKeyPress += (sender, args) =>
            {
                args.Cancel = true;
                Program.CancellationTokenSource.Cancel();
            };

            Program.TasksToAwait.Add(Task.Factory.StartNew(async () =>
            {
                var timeoutTask = Task.Factory.StartNew(async () =>
                {
                    while (!Program.CancellationTokenSource.IsCancellationRequested)
                        await Task.Delay(1000).ConfigureAwait(false);
                }, Program.CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current).Unwrap();

                while (!Program.CancellationTokenSource.IsCancellationRequested)
                {
                    var inputTask = Task.Factory.StartNew(async () => await System.Console.In.ReadLineAsync().ConfigureAwait(false), Program.CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current).Unwrap();
                    await Task.WhenAny(timeoutTask, inputTask).ConfigureAwait(false);

                    if (timeoutTask.IsCompleted)
                        return;

                    var input = await inputTask.ConfigureAwait(false);
                    input = input?.Trim();

                    if (input == null)
                        continue;

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                        Program.CancellationTokenSource.Cancel();
                    else
                        await LoggerService.LogMessageAsync("Invalid command.", ConsoleColor.Red).ConfigureAwait(false);
                }
            }, Program.CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current).Unwrap());
        }
    }
}