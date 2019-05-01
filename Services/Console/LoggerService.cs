using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.mscorlib.System.Threading.Tasks;
using TheDialgaTeam.Worktips.Discord.Bot.Services.IO;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Console
{
    public sealed class LoggerService : IInitializable, IBackgroundLoopingTask, IDisposable
    {
        public BackgroundLoopingTask RunningTask { get; private set; }

        private Program Program { get; }

        private FilePathService FilePathService { get; }

        private ConcurrentQueue<(TextWriter textWriter, ConsoleColor consoleColor, string message)> LoggerQueue { get; set; }

        private StreamWriter StreamWriter { get; set; }

        public LoggerService(Program program, FilePathService filePathService)
        {
            Program = program;
            FilePathService = filePathService;
        }

        public void Initialize()
        {
            LoggerQueue = new ConcurrentQueue<(TextWriter textWriter, ConsoleColor consoleColor, string message)>();
            StreamWriter = new StreamWriter(new FileStream(FilePathService.ConsoleLogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));

            RunningTask = Task.Factory.StartNewBackgroundLoopingTask(_ =>
            {
                while (LoggerQueue.TryDequeue(out var logger))
                    LogMessage(logger.textWriter, logger.consoleColor, logger.message);
            }, Program.CancellationTokenSource.Token);
        }

        public void LogMessage(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            LoggerQueue.Enqueue((System.Console.Out, consoleColor, message));
        }

        public void LogErrorMessage(Exception exception)
        {
            LoggerQueue.Enqueue((System.Console.Error, ConsoleColor.Red, exception.ToString()));
        }

        private void LogMessage(TextWriter writer, ConsoleColor consoleColor, string message)
        {
            if (message == null)
                return;

            try
            {
                System.Console.ForegroundColor = consoleColor;

                writer.WriteLine(message);
                writer.Flush();

                StreamWriter.WriteLine($"{DateTime.UtcNow:u} {message}");
                StreamWriter.Flush();
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.ResetColor();
            }
        }

        public void Dispose()
        {
            RunningTask?.Dispose();
            StreamWriter?.Dispose();
        }
    }
}