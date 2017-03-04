using Microsoft.Extensions.Configuration;
using OniBot.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OniBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new SynchronizationContext();

            AsyncPump.Run(a => MainAsync(args), args);
            Console.ReadKey();
        }

        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        private static async Task MainAsync(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
            };

            try
            {
                var commandLineConfig = ParseCommandline(args);
                var config = BuildConfig(commandLineConfig);

                await Task.Yield();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static IConfigurationRoot BuildConfig(IConfigurationRoot commandLineConfig)
        {
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
            return configuration;
        }

        private static IConfigurationRoot ParseCommandline(string[] args)
        {
            var switchMappings = new Dictionary<string, string>
                {
                    { "-environment", "environment" }
                };
            var commandLine = new ConfigurationBuilder();
            commandLine.AddCommandLine(args, switchMappings);
            var commandLineConfig = commandLine.Build();
            return commandLineConfig;
        }
    }
}