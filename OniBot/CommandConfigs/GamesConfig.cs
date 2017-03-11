using Newtonsoft.Json;
using OniBot.Interfaces;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    public class GamesConfig : CommandConfig
    {
        public List<string> Games { get; set; } = new List<string>();

        [JsonIgnore]
        public override string ConfigKey => "updategame";
    }
}
