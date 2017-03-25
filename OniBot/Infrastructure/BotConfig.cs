using Discord;

namespace OniBot.Interfaces
{
    public class BotConfig : IBotConfig
    {
        public LogSeverity LogLevel { get; set; }
        public bool AlwaysDownloadUsers { get; set; }
        public int MessageCacheSize { get; set; }
        public string Token { get; set; }
        public char PrefixChar { get; set; }
        public string ForgeKey { get; set; }
        public string ForgeSecret { get; set; }
        public int BotId { get; set; }
    }
}