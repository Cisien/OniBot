using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    public class Configuration
    {
        private static readonly object _fileReadWriteLock = new object();

        public static T Get<T>(string key) where T : class, new()
        {
            lock (_fileReadWriteLock)
            {
                if (!Directory.Exists("./config"))
                {
                    Directory.CreateDirectory("./config");
                }

                var configFile = $"./config/{key}.json";
                if (!File.Exists(configFile))
                {
                    File.WriteAllText(configFile, $"{{\r\n}}");
                }

                var configContents = File.ReadAllText(configFile);
                var config = new T();
                JsonConvert.PopulateObject(configContents, config, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                return config;
            }
        }

        public static void Write<T>(T data, string key) where T : class
        {
            lock (_fileReadWriteLock)
            {
                var configFile = $"./config/{key}.json";

                var config = JsonConvert.SerializeObject(data);
                File.WriteAllText(configFile, config);
            }
        }

        public static async Task Modify<T>(string key, Func<T, Task> action) where T : class, new()
        {
            var config = Get<T>(key);
            await action(config);
            Write(config, key);
        }

        public static Task Modify<T>(string key, Action<T> action) where T : class, new()
        {
            var config = Get<T>(key);
            action(config);
            Write(config, key);
            return Task.CompletedTask;
        }
    }
}
