using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OniBot.CommandConfigs;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Behaviors
{
    public class AnnounceBehavior : IBotBehavior
    {
        private readonly DiscordSocketClient _bot;
        private readonly ILogger<AnnounceBehavior> _logger;
        private readonly AnnounceConfig _config;

        public AnnounceBehavior(DiscordSocketClient bot, ILogger<AnnounceBehavior> logger, AnnounceConfig config)
        {
            _bot = bot;
            _logger = logger;
            _config = config;
        }

        public string Name => nameof(AnnounceBehavior);

        public Task RunAsync()
        {
            _bot.UserVoiceStateUpdated -= UserVoiceStateUpdated;
            _bot.ChannelUpdated -= ChannelUpdated;
            _bot.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _bot.ChannelUpdated += ChannelUpdated;
            return Task.CompletedTask;
        }

        private async Task ChannelUpdated(SocketChannel before, SocketChannel after)
        {
            if (!(before is SocketTextChannel beforeChannel) || !(after is SocketTextChannel afterChannel))
            {
                _logger.LogWarning($"before or after channels not vald text channels");
                return;
            }

            var channel = afterChannel.Guild.DefaultChannel;
            _config.Reload(channel.Guild.Id);
            if (!_config.Enabled)
            {
                return;
            }

            if (afterChannel.Topic != beforeChannel.Topic)
            {
                _logger.LogInformation($"Topic changed to { afterChannel.Topic}");

                await channel.SendMessageAsync($"Topic changed to { afterChannel.Topic}", true);
            }

            if (afterChannel.Name != afterChannel.Name)
            {
                _logger.LogInformation($"Channel name changed to { afterChannel.Name}");
                
                await channel.SendMessageAsync($"Channel name changed to { afterChannel.Name}", true);
            }
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            var guildUser = user as SocketGuildUser;
            RestUserMessage sentMessage = null;
            _logger.LogInformation($"{guildUser?.Guild?.Name}: {guildUser?.VoiceChannel?.Name}: {user.Username}: {user.Status}: {guildUser?.Nickname}: {guildUser?.VoiceState}");
            var name = string.IsNullOrWhiteSpace(guildUser.Nickname) ? user.Username : guildUser.Nickname;
            if (guildUser == null || string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning($"{user.Username} is not a valid guildUser");
                return;
            }

            if (before.VoiceChannel?.Name == after.VoiceChannel?.Name)
            {
                _logger.LogInformation($"Previous channel name matches the current channel name, doing nothing!");
                return;
            }

            var channel = guildUser.Guild.DefaultChannel;

            _config.Reload(channel.Guild.Id);

            if (!_config.Enabled)
            {
                return;
            }

            if (after.VoiceChannel != null &&_config.VoiceChannels.Contains(after.VoiceChannel.Id))
            {
                sentMessage = await channel.SendMessageAsync($"{name} joined {after.VoiceChannel.Name}!", true);
            }
            else if (guildUser.VoiceState == null || _config.VoiceChannels.Contains(before.VoiceChannel.Id))
            {
                sentMessage = await channel.SendMessageAsync($"{name} left {before.VoiceChannel.Name}", true);
            }
            await sentMessage?.DeleteAsync();
        }


        public Task StopAsync()
        {
            _bot.UserVoiceStateUpdated -= UserVoiceStateUpdated;
            _bot.ChannelUpdated -= ChannelUpdated;
            return Task.CompletedTask;
        }
    }
}
