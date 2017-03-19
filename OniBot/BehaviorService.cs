using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OniBot
{
    class BehaviorService
    {
        private IDependencyMap _map;
        private Dictionary<string, IBotBehavior> _behaviors = new Dictionary<string, IBotBehavior>();
        private ILogger _logger;

        public BehaviorService(IDependencyMap map, ILogger logger)
        {
            _map = map;
            _logger = logger;
        }

        public Task InstallAsync()
        {
            LoadBehaviors(_map);

            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

            foreach (var behavior in _behaviors)
            {
                try
                {
                    await behavior.Value.RunAsync().ConfigureAwait(false);
                    _logger.LogInformation($"Started behavior {behavior.Key}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                }
            }
        }

        public async Task StopAsync()
        {
            foreach (var behavior in _behaviors)
            {
                await behavior.Value.StopAsync();
            }
        }

        private void LoadBehaviors(IDependencyMap map)
        {
            var sMap = map as ServiceProviderDependencyMap;

            var assembly = Assembly.GetEntryAssembly();
            var interfaceType = typeof(IBotBehavior);

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

                    var instance = ActivatorUtilities.CreateInstance(sMap.GetProvider(), type) as IBotBehavior;
                    if (instance == null)
                    {
                        _logger.LogError($"Unable to create instance of behavior {type.FullName}");
                        continue;
                    }

                    _behaviors.Add(instance.Name, instance);
                    _logger.LogInformation($"Loaded behavior {instance.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                }
            }
        }
    }
}
