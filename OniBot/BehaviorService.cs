using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
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

        public async Task InstallAsync()
        {
            LoadBehaviors();

            await Task.Yield();
        }

        public async Task RunAsync()
        {

            await Task.Delay(TimeSpan.FromSeconds(10));

            foreach (var behavior in _behaviors)
            {
                try
                {
                    await behavior.Value.RunAsync();
                    _logger.LogInformation($"Started behavior {behavior.Key}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(), ex, ex.Message);
                }
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
                    var ctor = FindMostSpecificCtor(typeInfo);

                    IBotBehavior instance;
                    if (ctor == null)
                    {
                        instance = (IBotBehavior)Activator.CreateInstance(type);
                    }
                    else
                    {
                        var parameterInstances = new List<object> { };
                        var pars = ctor.GetParameters();
                        foreach (var par in pars)
                        {
                            _map.TryGet(par.ParameterType, out object result);

                            parameterInstances.Add(result);
                        }

                        instance = (IBotBehavior)Activator.CreateInstance(type, parameterInstances.ToArray());
                    }

                    if (instance == null)
                    {
                        _logger.LogError($"Unable to create instance of behavior {type.FullName}");
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

        private ConstructorInfo FindMostSpecificCtor(TypeInfo typeInfo)
        {
            foreach (var ctor in typeInfo.DeclaredConstructors)
            {
                if (ctor.GetParameters().Length > 0)
                {
                    return ctor;
                }
            }
            return null;
        }
    }
}
