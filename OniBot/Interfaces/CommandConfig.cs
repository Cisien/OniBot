using OniBot.Infrastructure;
using System.IO;
using System.Reflection;

namespace OniBot.Interfaces
{
    public abstract class CommandConfig
    {
        public abstract string ConfigKey { get; }
        public virtual void Reload(ulong? guild = null)
        {
            var type = GetType();
            var typeInfo = type.GetTypeInfo();
            var properties = typeInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var config = Configuration.Get(type, ConfigKey, guild);

            foreach (var prop in properties)
            {
                if (prop.SetMethod != null)
                {
                    prop.SetValue(this, prop.GetValue(config));
                }
            }
        }
    }
}