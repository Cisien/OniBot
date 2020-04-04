using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using OniBot.Interfaces;
using System;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using OniBot.Infrastructure;
using System.Collections.Generic;

namespace OniBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
            .ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables("BOT_")
                .AddCommandLine(args, new Dictionary<string, string>
                {
                    ["--environment"] = "Environment",
                    ["-e"] = "Environment"
                });
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false)
                .AddJsonFile($"config.{context.HostingEnvironment.EnvironmentName}.json", true);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets("OniBot");
                }
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.AddConsole(o =>
                {
                    o.DisableColors = true;
                    o.Format = Microsoft.Extensions.Logging.Console.ConsoleLoggerFormat.Systemd;
                    o.TimestampFormat = "o";
                });
                if (context.HostingEnvironment.IsDevelopment())
                {
                    logging.AddDebug();
                }
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices((context, services) =>
            {
                RegisterConfigInstances(services);

                var config = new BotConfig();
                context.Configuration.Bind(config);
                services.AddSingleton(config);
                services.AddSingleton<IBotConfig>(config);

                RegisterDiscordClient(services, config);

                services.AddSingleton<IVoiceService, AzureRestVoiceService>();
                services.AddSingleton<IBehaviorService, BehaviorService>();
                services.AddSingleton(provider => BuildCommandHandler(provider, config));

                services.AddHostedService<SocketDiscordBot>();
            })
            .UseConsoleLifetime();

            await host.RunConsoleAsync();
        }

        private static void RegisterDiscordClient(IServiceCollection services, BotConfig config)
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose, //config.LogLevel,
                AlwaysDownloadUsers = config.AlwaysDownloadUsers,
                MessageCacheSize = config.MessageCacheSize,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            services.AddSingleton<IDiscordClient>(client);
            services.AddSingleton(client);
        }

        private static ICommandHandler BuildCommandHandler(IServiceProvider provider, BotConfig config)
        {
            var logger = provider.GetService<ILogger<ICommandHandler>>();
            var commandService = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync,
                SeparatorChar = ' ',
                LogLevel = config.LogLevel
            });

            var discordLogger = new DiscordLogger<ICommandHandler>(logger);
            commandService.Log += discordLogger.OnLogAsync;

            var handler = ActivatorUtilities.CreateInstance(provider, typeof(CommandHandler), commandService) as ICommandHandler;
            return handler;
        }

        private static void RegisterConfigInstances(IServiceCollection services)
        {
            var assembly = Assembly.GetEntryAssembly();
            var interfaceType = typeof(CommandConfig);

            var exportedTypes = assembly.ExportedTypes;

            foreach (var type in exportedTypes)
            {
                try
                {
                    var typeInfo = type.GetTypeInfo();

                    if (!interfaceType.IsAssignableFrom(type) || typeInfo.IsInterface || typeInfo.IsAbstract)
                    {
                        continue;
                    }

                    services.AddTransient(type, (sp) =>
                    {
                        var instance = ActivatorUtilities.CreateInstance(sp, type) as CommandConfig;
                        instance.Reload();
                        return instance;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}