using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class AvatarConfig : CommandConfig
    {
        public Dictionary<string, string> Avatars { get; set; } = new Dictionary<string, string>();
        public bool Enabled { get; internal set; }

        [JsonIgnore]
        public override string ConfigKey => "avatar";

    }
}
