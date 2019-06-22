using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Microsoft.Extensions.DependencyInjection;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Bootstrap;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Console;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Discord;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;
using TheDialgaTeam.Worktips.Discord.Bot.Services.IO;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Rpc;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot
{
    public sealed class Program : IDisposable
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public CommandService CommandService { get; private set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public List<Task> TasksToAwait { get; private set; }

        public static async Task Main(string[] args)
        {
            var program = new Program();
            await program.Start(args).ConfigureAwait(false);
        }

        private async Task Start(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInterfacesAndSelfAsSingleton(this);
            serviceCollection.AddInterfacesAndSelfAsSingleton<FilePathService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<LoggerService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<BootstrapService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<ConfigService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<SqliteDatabaseService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<RpcService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<DiscordAppService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<ConsoleCommandService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            CommandService = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false });
            await CommandService.AddModulesAsync(Assembly.GetExecutingAssembly(), ServiceProvider).ConfigureAwait(false);

            CancellationTokenSource = new CancellationTokenSource();
            TasksToAwait = new List<Task>();

            try
            {
                ServiceProvider.InitializeServices();
                ServiceProvider.LateInitializeServices();

                Task.WaitAll(TasksToAwait.ToArray());

                ServiceProvider.DisposeServices();
            }
            catch (AggregateException ex)
            {
                var loggerService = ServiceProvider?.GetService<LoggerService>();

                if (loggerService != null)
                {
                    foreach (var exception in ex.InnerExceptions)
                    {
                        if (exception is TaskCanceledException)
                            continue;

                        await loggerService.LogErrorMessageAsync(exception).ConfigureAwait(false);
                    }
                }

                CancellationTokenSource?.Cancel();
                ServiceProvider?.DisposeServices();
                Environment.Exit(1);
                return;
            }
            catch (Exception ex)
            {
                var loggerService = ServiceProvider?.GetService<LoggerService>();

                if (loggerService != null)
                    await loggerService.LogErrorMessageAsync(ex).ConfigureAwait(false);

                CancellationTokenSource?.Cancel();
                ServiceProvider?.DisposeServices();
                Environment.Exit(1);
                return;
            }

            Environment.Exit(0);
        }

        public void Dispose()
        {
            ServiceProvider?.Dispose();
            ((IDisposable) CommandService)?.Dispose();
            CancellationTokenSource?.Dispose();

            foreach (var task in TasksToAwait)
                task.Dispose();
        }
    }
}