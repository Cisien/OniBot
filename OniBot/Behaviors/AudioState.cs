using Discord;
using Discord.Audio;
using Discord.Audio.Streams;

namespace OniBot.Behaviors
{
    internal class AudioState
    {
        public IAudioChannel Channel { get; set; }
        public IAudioClient Client { get; set; }
        public AudioOutStream Stream { get; set; }
    }
}