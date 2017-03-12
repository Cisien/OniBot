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
            string path;

            if (!guild.HasValue)
            {
                path = ConfigKey;
            }
            else
            {
                path = Path.Combine(guild.Value.ToString(), ConfigKey);
            }

            var config = Configuration.Get(type, path);

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