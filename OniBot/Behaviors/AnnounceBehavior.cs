using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OniBot.CommandConfigs;
using OniBot.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OniBot.Behaviors
{
    public class AnnounceBehavior : IBotBehavior
    {
        private readonly DiscordSocketClient _bot;
        private readonly ILogger<AnnounceBehavior> _logger;
        private readonly AnnounceConfig _config;
        private readonly IVoiceService _voiceService;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ConcurrentDictionary<ulong, IAudioClient> _joinedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1);
        private readonly Timer _keepaliveTimer;


        public AnnounceBehavior(DiscordSocketClient bot, ILogger<AnnounceBehavior> logger, AnnounceConfig config, IVoiceService voiceService, IHostApplicationLifetime appLifetime)
        {
            _bot = bot;
            _logger = logger;
            _config = config;
            _voiceService = voiceService;
            _appLifetime = appLifetime;
            _keepaliveTimer = new Timer(async (a) => await KeepAlive(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public string Name => nameof(AnnounceBehavior);

        public Task RunAsync()
        {
            _bot.UserVoiceStateUpdated -= UserVoiceStateUpdated;
            _bot.UserVoiceStateUpdated += UserVoiceStateUpdated;

            _keepaliveTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30));
            return Task.CompletedTask;
        }

        private Task KeepAlive()
        {
            foreach (var guild in _bot.Guilds)
            {
                var guildConfig = new AnnounceConfig();
                guildConfig.Reload(guild.Id);
                if (guildConfig.Enabled && !guildConfig.UseTts)
                {
                    CreateAudioClient(guild, guildConfig.AudioChannel);
                }
            }
            return Task.CompletedTask;
        }

        private Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.IsBot)
            {
                return Task.CompletedTask;
            }
            _config.Reload((user as SocketGuildUser).Guild.Id);

            if (!_config.Enabled)
            {
                return Task.CompletedTask;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var guildUser = user as SocketGuildUser;
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

                    if (after.VoiceChannel != null && _config.VoiceChannels.Contains(after.VoiceChannel.Id))
                    {
                        await SendMessageAsync(guildUser, $"{name} joined {after.VoiceChannel.Name}!");
                    }
                    else if (guildUser.VoiceState == null || _config.VoiceChannels.Contains(before.VoiceChannel.Id))
                    {
                        await SendMessageAsync(guildUser, $"{name} left {before.VoiceChannel.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                }
            });

            return Task.CompletedTask;
        }

        private async Task SendMessageAsync(SocketGuildUser user, string message)
        {

            if (_config.UseTts)
            {
                var channel = user.Guild.DefaultChannel;
                var sentMessage = await channel.SendMessageAsync(message, true);
                await sentMessage?.DeleteAsync();
            }
            else
            {
                await _sync.WaitAsync(TimeSpan.FromSeconds(30));
                var audioClient = _joinedChannels[user.Guild.Id];
                try
                {
                    var audio = await _voiceService.ToVoice(message);
                    using var audioStream = audioClient.CreatePCMStream(AudioApplication.Music);

                    await audioStream.WriteAsync(audio);
                    await audioStream.FlushAsync();
                }
                finally
                {
                    _sync.Release();
                }
            }
        }

        private void CreateAudioClient(SocketGuild guild, ulong audioChannelId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if(_joinedChannels.TryGetValue(guild.Id, out var audioClient))
                    {
                        if(audioClient.ConnectionState == ConnectionState.Connected 
                        || audioClient.ConnectionState == ConnectionState.Connecting)
                        {
                            return;
                        }
                    }

                    var voiceChannel = guild.GetVoiceChannel(audioChannelId);
                    audioClient = await voiceChannel.ConnectAsync();
                    
                    _joinedChannels[guild.Id] = audioClient;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                }
            });
        }

        public async Task StopAsync()
        {
            _bot.UserVoiceStateUpdated -= UserVoiceStateUpdated;

            if (_config.Enabled && !_config.UseTts)
            {
                foreach (var client in _joinedChannels)
                {
                    var voiceClient = client.Value;
                    await voiceClient.StopAsync();
                    voiceClient.Dispose();
                }
            }
        }
    }
}
