using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Console.Internal;

namespace OniBot.Infrastructure.Logger
{
    public class CustomConsoleLoggerProcessor : IDisposable
    {
        private const int _maxQueuedMessages = 1024;

        private readonly BlockingCollection<LogMessageEntry> _messageQueue = new BlockingCollection<LogMessageEntry>(_maxQueuedMessages);
        private readonly Task _outputTask;

        public IConsole Console;

        public CustomConsoleLoggerProcessor()
        {
            // Start Console message queue processor
            _outputTask = Task.Factory.StartNew(
                ProcessLogQueue,
                this,
                TaskCreationOptions.LongRunning);
        }

        public virtual void EnqueueMessage(LogMessageEntry message)
        {
            _messageQueue.Add(message);
        }

        // for testing
        internal virtual void WriteMessage(LogMessageEntry message)
        {
            if (message.LevelString != null)
            {
                Console.Write(message.LevelString, message.LevelBackground, message.LevelForeground);
            }

            Console.Write(message.Message, message.MessageColor, message.MessageColor);
            Console.Flush();
        }

        private void ProcessLogQueue()
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable())
            {
                WriteMessage(message);
            }
        }

        private static void ProcessLogQueue(object state)
        {
            var consoleLogger = (CustomConsoleLoggerProcessor)state;

            consoleLogger.ProcessLogQueue();
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputTask.Wait(1500); // with timeout in-case Console is locked by user input
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        }
    }
}