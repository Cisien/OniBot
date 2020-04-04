using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class AnnounceConfig : CommandConfig
    {
        public bool Enabled { get; set; }

        public List<ulong> VoiceChannels { get; set; } = new List<ulong>();
        public ulong AudioChannel { get; set; }

        public bool UseTts { get; set; }

        [JsonIgnore]
        public override string ConfigKey => nameof(AnnounceConfig);

    }
}
