using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.IO;

namespace TheDialgaTeam.Worktips.Discord.Bot.Services.Console
{
    public sealed class LoggerService : IInitializable, IDisposable
    {
        private Program Program { get; }

        private FilePathService FilePathService { get; }

        private StreamWriter StreamWriter { get; set; }

        private SemaphoreSlim SemaphoreSlim { get; set; }

        public LoggerService(Program program, FilePathService filePathService)
        {
            Program = program;
            FilePathService = filePathService;
        }

        public void Initialize()
        {
            StreamWriter = new StreamWriter(new FileStream(FilePathService.ConsoleLogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            SemaphoreSlim = new SemaphoreSlim(1, 1);
        }

        public void LogMessage(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            LogMessageAsync(System.Console.Out, consoleColor, message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task LogMessageAsync(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            await LogMessageAsync(System.Console.Out, consoleColor, message).ConfigureAwait(false);
        }

        public void LogErrorMessage(Exception exception)
        {
            LogMessageAsync(System.Console.Error, ConsoleColor.Red, exception.ToString()).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task LogErrorMessageAsync(Exception exception)
        {
            await LogMessageAsync(System.Console.Error, ConsoleColor.Red, exception.ToString()).ConfigureAwait(false);
        }

        private async Task LogMessageAsync(TextWriter writer, ConsoleColor consoleColor, string message)
        {
            try
            {
                await SemaphoreSlim.WaitAsync(Program.CancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                System.Console.ForegroundColor = consoleColor;

                await writer.WriteLineAsync(message).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);

                await StreamWriter.WriteLineAsync($"{DateTime.UtcNow:s} {message}").ConfigureAwait(false);
                await StreamWriter.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        public void Dispose()
        {
            StreamWriter?.Dispose();
            SemaphoreSlim?.Dispose();
        }
    }
}