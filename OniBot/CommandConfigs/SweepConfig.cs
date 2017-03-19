using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class SweepConfig : CommandConfig
    {
        public Dictionary<ulong, string> Equiped { get; set; } = new Dictionary<ulong, string>();

        [JsonIgnore]
        public override string ConfigKey => "sweep";
    }
}
