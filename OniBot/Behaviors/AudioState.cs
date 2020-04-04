using Discord;
using Discord.Audio;
using Discord.Audio.Streams;
using System;

namespace OniBot.Behaviors
{
    internal class AudioState : IDisposable
    {
        private bool disposedValue;

        public IAudioChannel Channel { get; set; }
        public IAudioClient Client { get; set; }
        public AudioOutStream Stream { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stream?.Dispose();
                    Client?.Dispose();
                    Channel?.DisconnectAsync();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}