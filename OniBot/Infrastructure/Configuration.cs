using Newtonsoft.Json;
using OniBot.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    public class Configuration
    {
        private static readonly object _fileReadWriteLock = new object();

        public static T Get<T>(string key, ulong? guild = null) where T : CommandConfig
        {
            return (T)Get(typeof(T), key, guild);
        }

        public static object Get(Type type, string key, ulong? guild = null)
        {
            lock (_fileReadWriteLock)
            {
                var directory = Path.Combine(".", "config", guild?.ToString() ?? string.Empty);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var configFile = Path.Combine(directory, $"{key}.json");
                if (!File.Exists(configFile))
                {
                    File.WriteAllText(configFile, $"{{\r\n}}");
                }

                var configContents = File.ReadAllText(configFile);
                var config = Activator.CreateInstance(type);
                JsonConvert.PopulateObject(configContents, config, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                return config;
            }
        }

        public static string GetJson<T>(string key, ulong? guild = null) where T : CommandConfig
        {
            var config = Get(typeof(T), key, guild);
            return JsonConvert.SerializeObject(config, Formatting.Indented);
        }

        public static void Write<T>(T data, string key, ulong? guild = null) where T : CommandConfig
        {
            lock (_fileReadWriteLock)
            {
                var configFile = Path.Combine(".", "config", guild?.ToString() ?? string.Empty, $"{key}.json");

                var config = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(configFile, config);
            }
        }

        public static async Task Modify<T>(string key, Func<T, Task> action, ulong? guild = null) where T : CommandConfig
        {
            var config = Get<T>(key, guild);
            await action(config);
            Write(config, key, guild);
        }

        public static Task Modify<T>(string key, Action<T> action, ulong? guild = null) where T : CommandConfig
        {
            var config = Get<T>(key, guild);
            action(config);
            Write(config, key, guild);
            return Task.CompletedTask;
        }
    }
}
