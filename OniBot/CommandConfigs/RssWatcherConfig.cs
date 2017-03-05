using System;
using System.Collections.Generic;
using System.Text;

namespace OniBot.CommandConfigs
{
    class RssWatcherConfig
    {
        public List<RssTracker> RssTracker = new List<RssTracker>();
        public int CheckFrequencyMinutes { get; set; } = 15;
    }

    public class RssTracker
    {
        public ulong Channel { get; set; }

        public string RssFeed { get; set; }

        public Dictionary<int, DateTimeOffset> ChangeTracker { get; set; } = new Dictionary<int, DateTimeOffset>();
    }
}
