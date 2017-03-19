using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    public class HostingEnvironment
    {
        public string Environment { get; private set; } = "production";
        public Type Startup { get; set; }
        public ServiceProviderDependencyMap Map { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        public Type Bot { get; set; }
        public IEnumerable<KeyValuePair<string, string>> CommandLineOptions { get; set; }

        /// <summary>
        /// Configures and begins hosting the provided bot.
        /// </summary>
        /// <param name="token">This Task will not complete until the cancelation token is canceled.</param>
        /// <returns>an awaitable task that will continue to run for the lifetime of the bot.</returns>
        public async Task RunAsync(CancellationToken token)
        {
            var serviceProvider = Map.GetProvider();
            var environmentOptions = CommandLineOptions.Where(a => a.Key == "environment");
            if (environmentOptions.Count() == 1)
            {
                Environment = environmentOptions.Single().Value;
            }

            var startup = ActivatorUtilities.CreateInstance(serviceProvider, Startup, this);

            var sti = Startup.GetTypeInfo();
            var configureMethod = sti.GetMethod("Configure");
            var configureServicesMethod = sti.GetMethod("ConfigureServices");

            if (configureMethod == null)
            {
                throw new InvalidOperationException("Startup class is missing a Configure method");
            }

            if (configureServicesMethod == null)
            {
                throw new InvalidOperationException("Startup class is mssing a ConfigureServices method");
            }

            configureServicesMethod.Invoke(startup, new object[] { Map, LoggerFactory });
            serviceProvider = Map.GetProvider();
            configureMethod.Invoke(startup, GetMethodParameterInstances(configureMethod));

            var bot = ActivatorUtilities.CreateInstance(serviceProvider, Bot) as IDiscordBot;
            Map.Add(bot);

            using (bot)
            {
                await bot.RunBotAsync().ConfigureAwait(false);
                try
                {
                    await Task.Delay(-1, token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        private object[] GetMethodParameterInstances(MethodInfo method)
        {
            var parameters = new List<object>();

            foreach (var param in method.GetParameters())
            {
                if (!Map.TryGet(param.ParameterType, out object paramToAdd))
                {
                    throw new InvalidOperationException($"{param.ParameterType} is not registered in DI.");
                }
                parameters.Add(paramToAdd);
            }

            return parameters.ToArray();
        }
    }
}
