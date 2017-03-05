using Newtonsoft.Json;
using System.IO;

namespace OniBot.Infrastructure
{
    public class Configuration
    {
        private static readonly object _fileReadWriteLock = new object();

        public static T Get<T>(string key) where T : class
        {
            lock (_fileReadWriteLock)
            {
                if (!Directory.Exists("./config"))
                {
                    Directory.CreateDirectory("./config");
                }

                var configFile = $"./config/{key}.json";
                if (!File.Exists(Path.Combine(configFile)))
                {
                    File.WriteAllText(configFile, $"{{\r\n}}");
                }
                var configContents = File.ReadAllText(configFile);
                var config = JsonConvert.DeserializeObject<T>(configContents);

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
    }
}
