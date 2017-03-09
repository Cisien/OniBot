using Discord;
using Discord.Commands;
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

        public BehaviorService(IDependencyMap map)
        {
            _map = map;
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
                    DiscordBot.Log(nameof(RunAsync), LogSeverity.Info, $"Started behavior {behavior.Key}");
                }
                catch (Exception ex)
                {
                    DiscordBot.Log(nameof(RunAsync), LogSeverity.Error, ex.ToString());
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
                        DiscordBot.Log(nameof(LoadBehaviors), LogSeverity.Error, $"Unable to create instance of behavior {type.FullName}");
                    }

                    _behaviors.Add(instance.Name, instance);
                    DiscordBot.Log(nameof(LoadBehaviors), LogSeverity.Info, $"Loaded behavior {instance.Name}");

                }
                catch (Exception ex)
                {
                    DiscordBot.Log(nameof(LoadBehaviors), LogSeverity.Error, ex.ToString());
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
