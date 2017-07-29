using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OniBot.Infrastructure;
using OniBot.Infrastructure.Logger;
using OniBot.Interfaces;
using System;
using System.Reflection;

namespace OniBot
{
    class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public Startup(HostingEnvironment hostingEnvironment)
        {
            var environment = hostingEnvironment.Environment?.ToLower() ?? "production";
            var config = new ConfigurationBuilder();
            config.AddJsonFile("config.json", false);
            config.AddJsonFile($"config.{environment}.json", true);
            config.AddEnvironmentVariables();
            if (environment == "development")
            {
                config.AddUserSecrets("OniBot");
            }
            config.AddInMemoryCollection(hostingEnvironment.CommandLineOptions);

            var configuration = config.Build();
#if DEBUG
            foreach (var key in configuration.AsEnumerable())
            {
                Console.WriteLine($"{key.Key.PadRight(40)}: {key.Value}");
            }
#endif
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services, ILoggerFactory loggerFactory)
        {
            var config = new BotConfig();
            ConfigurationBinder.Bind(Configuration, config);

            loggerFactory.AddCustomConsole(LogLevel.Trace);
            if (Configuration["environment"] == "development")
            {
                loggerFactory.AddDebug(LogLevel.Trace);
            }

            var logger = loggerFactory.CreateLogger("Common");
            services.AddSingleton(logger);
            RegisterConfigInstances(services, logger);

            var commandService = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync,
                SeparatorChar = ' ',
                LogLevel = config.LogLevel
            });
            commandService.Log += DiscordBot.OnLogAsync;

            var behaviorService = new BehaviorService(services, logger);
            var commandHanlder = new CommandHandler(commandService, config, logger);
            
            services.AddSingleton(config);
            services.AddSingleton(behaviorService);
            services.AddSingleton<ICommandHandler>(commandHanlder);
        }

        public void Configure()
        {

        }

        private void RegisterConfigInstances(IServiceCollection map, ILogger logger)
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

                    map.AddTransient(type, (sp) =>
                    {
                        var instance = ActivatorUtilities.CreateInstance(sp, type) as CommandConfig;
                        instance.Reload();
                        return instance;
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex);
                    throw;
                }
            }
        }
    }
}
