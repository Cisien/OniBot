using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;

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
                config.AddUserSecrets();
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<BotConfig>(Configuration);
            services.AddSingleton(a =>
            {
                return new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    SeparatorChar = '|'
                });
            });
            services.AddSingleton<ICommandHandler, CommandHandler>();
            services.AddSingleton<IDiscordBot, DiscordBot>();
        }
    }
}
