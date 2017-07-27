using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OniBot.Infrastructure;
using System.Collections.Generic;

namespace OniBot
{
    public static class HostingEnvironmentBuilderExtensions
    {
        public static HostingEnvironment UseStartup<T>(this HostingEnvironment builder) where T : class
        {
            builder.Startup = typeof(T);
            return builder;
        }

        public static HostingEnvironment UseLoggerFactory(this HostingEnvironment builder, ILoggerFactory factory)
        {
            builder.LoggerFactory = factory;
            return builder;
        }

        public static HostingEnvironment UseDependencyMap(this HostingEnvironment builder, IServiceCollection map)
        {
            builder.Map = map;
            return builder;
        }

        public static HostingEnvironment UseBot<T>(this HostingEnvironment builder) where T : IDiscordBot
        {
            builder.Bot = typeof(T);
            return builder;
        }

        public static HostingEnvironment UseCommandLineOptions(this HostingEnvironment builder, string[] args) {

            var switchMappings = new Dictionary<string, string>
                {
                    { "-environment", "environment" }
                };
            var commandLine = new ConfigurationBuilder();
            commandLine.AddCommandLine(args, switchMappings);
            var commandLineConfig = commandLine.Build();
            builder.CommandLineOptions = commandLineConfig.AsEnumerable();

            return builder;
        }

    }
}
