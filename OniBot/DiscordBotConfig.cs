using System;
using Discord;

namespace OniBot
{
    public class DiscordBotConfig : IDiscordBotConfig
    {
        public string[] Games { get; set; } = new string[] { "OxygenNotIncluded" };
        public LogSeverity LogLevel { get; set; }
        public bool AlwaysDownloadUsers { get; set; }
        public int MessageCacheSize { get; set; }
        public string Token { get; set; }
    }
}