﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using OniBot.CommandConfigs;
using OniBot.Infrastructure;
using OniBot.Interfaces;
using System.Threading.Tasks;

namespace OniBot.Commands
{
    [Group("announce")]
    [Summary("A group of commands related to user presnece announcements")]
    [ConfigurationPrecondition]
    public class AnnounceCommand : ModuleBase<SocketCommandContext>, IBotCommand
    {
        private readonly AnnounceConfig _config;

        public AnnounceCommand(AnnounceConfig config)
        {
            _config = config;
        }

        [Command("show")]
        [Summary("Sends you the current running Announce config.")]
        public async Task Show()
        {
            var cfg = Configuration.GetJson<AnnounceConfig>(_config.ConfigKey, Context.Guild.Id);
            await Context.User.SendMessageAsync(Format.Code(cfg)).ConfigureAwait(false);
        }
        [Command("enable")]
        [Summary("Enables announcements")]
        public async Task Enable([Summary("Whether to enable or disable presence announcements.")]bool enable)
        {

            await Configuration.Modify<AnnounceConfig>(_config.ConfigKey, a =>
            {
                a.Enabled = enable;
            }, Context.Guild.Id).ConfigureAwait(false);

            await ReplyAsync($"Presence announcements are {(enable ? "Enabled" : "Disabled")}");
        }

        [Command("remove")]
        [Summary("Removes a channel from announcing presence updates.")]
        public async Task Remove([Summary("The channel to remove.")] SocketVoiceChannel channel)
        {
            await Configuration.Modify<AnnounceConfig>(_config.ConfigKey, a =>
            {
                if (a.VoiceChannels.Contains(channel.Id))
                {
                    a.VoiceChannels.Remove(channel.Id);
                }
            }, Context.Guild.Id).ConfigureAwait(false);

            await ReplyAsync($"{channel.Name} will no longer announce presence changes.").ConfigureAwait(false);
        }

        [Command("usetts")]
        [Summary("Toggles between discord tts and a custom tts")]
        public async Task UseTts(bool useTts)
        {
            await Configuration.Modify<AnnounceConfig>(_config.ConfigKey, a =>
            {
                a.UseTts = useTts;
            }, Context.Guild.Id).ConfigureAwait(false);

            await ReplyAsync($"Using discord's TTS: {useTts}").ConfigureAwait(false);
        }

        [Command("voicechannel")]
        [Summary("Sets the custom tts voice channel")]
        public async Task SetVoiceChannel(SocketVoiceChannel voiceChannel)
        {
            await Configuration.Modify<AnnounceConfig>(_config.ConfigKey, a =>
            {
                a.AudioChannel = voiceChannel.Id;
            }, Context.Guild.Id).ConfigureAwait(false);

            await ReplyAsync($"Custom TTS will be spoken in the {voiceChannel.Name} channel.").ConfigureAwait(false);
        }

        [Command("add")]
        [Summary("Adds a channel for announcing presence updates")]
        public async Task Add([Summary("The channel to add.")]SocketVoiceChannel channel)
        {
            await Configuration.Modify<AnnounceConfig>(_config.ConfigKey, a =>
            {
                if (!a.VoiceChannels.Contains(channel.Id))
                {
                    a.VoiceChannels.Add(channel.Id);
                }
            }, Context.Guild.Id).ConfigureAwait(false);

            await ReplyAsync($"{channel.Name} will now announce presence changes.").ConfigureAwait(false);
        }

        [Command("set")]
        [Summary("sets the config to a blob of json")]
        public async Task Set([Remainder][Summary("The Json")] string theJson)
        {
            await Configuration.Modify<AnnounceConfig>(_config.ConfigKey, a =>
            {
                JsonConvert.PopulateObject(theJson, a);
            }, Context.Guild.Id).ConfigureAwait(false);

        }
    }
}
