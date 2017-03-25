using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class ChatConfig : CommandConfig
    {
        [JsonIgnore]
        public override string ConfigKey => nameof(ChatConfig);

        public List<ulong> AllowedChannels { get; set; } = new List<ulong>();
    }
}
