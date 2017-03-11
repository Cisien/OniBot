using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OniBot
{
    class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        public Startup(string[] args)
        {
            var switchMappings = new Dictionary<string, string>
                {
                    { "-environment", "environment" }
                };
            var commandLine = new ConfigurationBuilder();
            commandLine.AddCommandLine(args, switchMappings);
            var commandLineConfig = commandLine.Build();

            var environment = commandLineConfig["environment"]?.ToLower() ?? "production";
            var config = new ConfigurationBuilder();
            config.AddJsonFile("config.json", false);
            config.AddJsonFile($"config.{environment}.json", true);
            config.AddEnvironmentVariables();
            if (environment == "development")
            {
                config.AddUserSecrets("OniBot");
            }
            config.AddInMemoryCollection(commandLineConfig.AsEnumerable());

            var configuration = config.Build();
#if DEBUG
            foreach (var key in configuration.AsEnumerable())
            {
                Console.WriteLine($"{key.Key.PadRight(40)}: {key.Value}");
            }
#endif
            Configuration = configuration;
        }

        public void ConfigureServices(IDependencyMap services)
        {
            var config = new BotConfig();
            ConfigurationBinder.Bind(Configuration, config);

            var provider = new LoggerFactory();
            provider.AddConsole(LogLevel.Information);
            if (Configuration["environment"] == "development")
            {
                provider.AddDebug(LogLevel.Trace);
            }

            var logger = provider.CreateLogger("Common");
            services.Add(logger);
            RegisterConfigInstances(services);

            var commandService = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync,
                SeparatorChar = ' '
            });

            var behaviorService = new BehaviorService(services, logger);
            var commandHanlder = new CommandHandler(commandService, config, logger);
            var bot = new DiscordBot(config, commandHanlder, services, behaviorService, logger);

            services.Add(services);
            services.Add(config);
            services.Add(commandService);
            services.Add(behaviorService);
            services.Add<ICommandHandler>(commandHanlder);
            services.Add<IDiscordBot>(bot);
        }

        private void RegisterConfigInstances(IDependencyMap map)
        {
            var spMap = map as ServiceProviderDependencyMap;
            var logger = spMap.Get<ILogger>();
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

                    var instance = Activator.CreateInstance(type) as CommandConfig;
                    instance.Reload();

                    spMap.Add(type, instance);
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
