using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OniBot
{
    public class BehaviorService: IBehaviorService
    {
        private readonly IServiceProvider _provider;
        private readonly Dictionary<string, IBotBehavior> _behaviors = new Dictionary<string, IBotBehavior>();
        private readonly ILogger<BehaviorService> _logger;

        public BehaviorService(IServiceProvider provider, ILogger<BehaviorService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public Task InstallAsync()
        {
            LoadBehaviors();

            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
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

        private void LoadBehaviors()
        {

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

                    if (!(ActivatorUtilities.CreateInstance(_provider, type) is IBotBehavior instance))
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
