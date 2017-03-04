using Discord;

namespace OniBot
{
    public interface IDiscordBotConfig
    {
        string[] Games { get; set; }
        LogSeverity LogLevel { get; set; }
        bool AlwaysDownloadUsers { get; set; }
        int MessageCacheSize { get; set; }
        string Token { get; set; }
    }
}