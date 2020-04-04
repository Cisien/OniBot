using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OniBot.CommandConfigs;
using OniBot.Interfaces;
using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<ulong, AudioState> _joinedChannels = new ConcurrentDictionary<ulong, AudioState>();
        private readonly Timer _keepaliveTimer;
        private readonly ConcurrentQueue<Message> _voiceQueue = new ConcurrentQueue<Message>();
        private Task _queueWorker;
        private bool _processQueue = true;

        public string Name => nameof(AnnounceBehavior);

        public AnnounceBehavior(DiscordSocketClient bot, ILogger<AnnounceBehavior> logger, AnnounceConfig config, IVoiceService voiceService, IHostApplicationLifetime appLifetime)
        {
            _bot = bot;
            _logger = logger;
            _config = config;
            _voiceService = voiceService;
            _appLifetime = appLifetime;
            _keepaliveTimer = new Timer(async (a) => await KeepAlive(), null, Timeout.Infinite, Timeout.Infinite);
            _queueWorker = new Task(async a => await QueueWorker(), CancellationToken.None, TaskCreationOptions.LongRunning);
        }

        private async Task QueueWorker()
        {
            while (_processQueue)
            {
                try
                {
                    if (!_voiceQueue.TryDequeue(out var message))
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    if (!_joinedChannels.TryGetValue(message.GuildId, out var audioState))
                    {
                        _logger.LogWarning($"No audio state available for {message.GuildId}");
                        await Task.Delay(100);
                        continue;
                    }

                    _logger.LogInformation("Sending message to discord");
                    await message.Audio.CopyToAsync(audioState.Stream);
                    await audioState.Stream.FlushAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing queue: {ex.Message}");
                }

                await Task.Delay(100);
            }
        }

        public Task RunAsync()
        {
            _bot.UserVoiceStateUpdated -= UserVoiceStateUpdated;
            _bot.UserVoiceStateUpdated += UserVoiceStateUpdated;

            _keepaliveTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
            _queueWorker.Start();
            return Task.CompletedTask;
        }

        private Task KeepAlive()
        {
            foreach (var guild in _bot.Guilds)
            {
                try
                {
                    var guildConfig = new AnnounceConfig();
                    guildConfig.Reload(guild.Id);
                    if (guildConfig.Enabled && !guildConfig.UseTts && guildConfig.AudioChannel != default)
                    {
                        CreateAudioClient(guild, guildConfig.AudioChannel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Failed to join audio for {guild.Name}: {ex.Message}");
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
                var audio = await _voiceService.ToVoice(message);
                _voiceQueue.Enqueue(new Message { GuildId = user.Guild.Id, Audio = audio });
            }
        }

        private void CreateAudioClient(SocketGuild guild, ulong audioChannelId)
        {
            if(!guild.IsConnected)
            {
                _logger.LogWarning($"bot is not Connected to this guild, trying later");
                return;
            }
            if (_bot.ConnectionState != ConnectionState.Connected)
            {
                _logger.LogWarning($"bot is not Connected to discord, trying later");
                return;
            }

            if (_bot.LoginState != LoginState.LoggedIn)
            {
                _logger.LogWarning($"bot is not logged in, trying later");
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    if (_joinedChannels.TryGetValue(guild.Id, out var audioState))
                    {
                        if (audioState.Client.ConnectionState == ConnectionState.Connected
                        || audioState.Client.ConnectionState == ConnectionState.Connecting)
                        {
                            return;
                        }
                    }

                    audioState = new AudioState();
                    var voiceChannel = guild.GetVoiceChannel(audioChannelId);
                    audioState.Channel = voiceChannel;
                    audioState.Client = await voiceChannel.ConnectAsync();
                    audioState.Stream = audioState.Client.CreatePCMStream(AudioApplication.Music, null, 100, 0);

                    _joinedChannels[guild.Id] = audioState;

                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Guild failed to create an audio client: {guild.Name}: {ex.Message}");

                }
            });
        }

        public async Task StopAsync()
        {
            _processQueue = false;
            _bot.UserVoiceStateUpdated -= UserVoiceStateUpdated;

            if (_config.Enabled && !_config.UseTts)
            {
                foreach (var client in _joinedChannels)
                {
                    var voiceState = client.Value;
                    await voiceState.Client.StopAsync();
                    voiceState.Client.Dispose();
                    voiceState.Stream.Dispose();
                }
            }
        }
    }
}
