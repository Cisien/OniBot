using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OniBot.Infrastructure;
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

        public void ConfigureServices(ServiceProviderDependencyMap services, ILoggerFactory loggerFactory)
        {
            var config = new BotConfig();
            ConfigurationBinder.Bind(Configuration, config);

            loggerFactory.AddConsole(LogLevel.Trace);
            if (Configuration["environment"] == "development")
            {
                loggerFactory.AddDebug(LogLevel.Trace);
            }

            var logger = loggerFactory.CreateLogger("Common");
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

            services.Add(services);
            services.Add<IDependencyMap>(services);
            services.Add(config);
            services.Add(commandService);
            services.Add(behaviorService);
            services.Add<ICommandHandler>(commandHanlder);
        }

        public void Configure()
        {

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

                    spMap.AddTransientFactory(type, () =>
                    {
                        var instance = Activator.CreateInstance(type) as CommandConfig;
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
