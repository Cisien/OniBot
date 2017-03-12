﻿using Discord.Commands;
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
        public string Environment { get; set; }
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
            Environment = CommandLineOptions.SingleOrDefault(a => a.Key == "environment").Value;

            var startup = ActivatorUtilities.CreateInstance(serviceProvider, Startup);

            var sti = Startup.GetTypeInfo();
            var configureMethod = sti.GetMethod("Configure");
            var configureServicesMethod = sti.GetMethod("ConfigureServices");

            configureServicesMethod.Invoke(startup, new object[] { Map, LoggerFactory });
            configureMethod.Invoke(startup, GetMethodParameterInstances(configureMethod));

            var bot = ActivatorUtilities.CreateInstance(serviceProvider, Bot) as IDiscordBot;
            Map.Add(bot);
            Map.Add(bot as DiscordBot);

            using (bot)
            {
                await bot.RunBotAsync();
                await Task.Delay(-1, token);
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