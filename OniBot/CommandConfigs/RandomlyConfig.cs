using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class RandomlyConfig : CommandConfig
    {
        public List<ImageMessage> RandomMessages { get; set; } = new List<ImageMessage>();
        public int MinMessages { get; set; }
        public int MaxMessages { get; set; }

        [JsonIgnore]
        public override string ConfigKey => "randomly";
    }

    public class ImageMessage
    {
        public string Message { get; set; }
        public string Image { get; set; }
    }
}
