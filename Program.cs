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
using TheDialgaTeam.Worktips.Discord.Bot.Services.RPC;
using TheDialgaTeam.Worktips.Discord.Bot.Services.Setting;

namespace TheDialgaTeam.Worktips.Discord.Bot
{
    public sealed class Program
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public CommandService CommandService { get; private set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public static async Task Main(string[] args)
        {
            var program = new Program();
            await program.Start(args).ConfigureAwait(false);
        }

        private async Task Start(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(this);
            serviceCollection.AddInterfacesAndSelfAsSingleton<FilePathService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<LoggerService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<BootstrapService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<SettingService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<SqliteDatabaseService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<RpcService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<DiscordAppService>();
            serviceCollection.AddInterfacesAndSelfAsSingleton<ConsoleCommandService>();

            CancellationTokenSource = new CancellationTokenSource();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            CommandService = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false });
            await CommandService.AddModulesAsync(Assembly.GetExecutingAssembly(), ServiceProvider).ConfigureAwait(false);

            ServiceProvider.InitializeServices();

            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            var services = ServiceProvider.GetServices<IBackgroundLoopingTask>();
            var awaitableTasks = new List<Task>();

            foreach (var backgroundLoopingTask in services)
                awaitableTasks.Add(backgroundLoopingTask.RunningTask);

            try
            {
                await Task.WhenAll(awaitableTasks).ConfigureAwait(false);
            }
            finally
            {
                ServiceProvider.DisposeServices();
                ServiceProvider.Dispose();

                Environment.Exit(0);
            }
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            CancellationTokenSource.Cancel();
        }
    }
}