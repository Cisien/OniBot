using Newtonsoft.Json;
using OniBot.Interfaces;
using System;
using System.Collections.Generic;

namespace OniBot.CommandConfigs
{
    class RssWatcherConfig : CommandConfig
    {
        public List<RssTracker> RssTracker = new List<RssTracker>();
        public int CheckFrequencyMinutes { get; set; } = 15;

        [JsonIgnore]
        public override string ConfigKey => "rsswatcher";

    }

    public class RssTracker
    {
        public ulong Channel { get; set; }

        public string RssFeed { get; set; }

        public Dictionary<int, DateTimeOffset> ChangeTracker { get; set; } = new Dictionary<int, DateTimeOffset>();
    }
}
