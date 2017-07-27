using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class AnnounceConfig : CommandConfig
    {
        public bool Enabled { get; set; }

        public List<ulong> VoiceChannels { get; set; } = new List<ulong>();

        [JsonIgnore]
        public override string ConfigKey => nameof(AnnounceConfig);

    }
}
