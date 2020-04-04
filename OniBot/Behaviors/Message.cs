using System;
using System.IO;

namespace OniBot.Behaviors
{
    internal class Message : IDisposable
    {
        private bool disposedValue;

        public ulong GuildId { get; set; }
        public Stream Audio { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Audio.Dispose();
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