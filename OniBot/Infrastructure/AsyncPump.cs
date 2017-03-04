using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OniBot.Infrastructure
{
    public class AsyncPump
    {
        public delegate Task SingleArgumentDelegate(object args);
        public static void Run(SingleArgumentDelegate func, object args)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var prevCtx = SynchronizationContext.Current;

            try
            {
                var syncCtx = new SingleThreadSynchronizationContext();
                var task = func(args).ContinueWith(a =>
                {
                    syncCtx.Complete();
                }, TaskScheduler.Default);

                syncCtx.RunOnCurrentThread();

                if(task.IsFaulted) {
                    var edi = ExceptionDispatchInfo.Capture(task.Exception);
                    edi.Throw();
                }

                task.GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        private class SingleThreadSynchronizationContext : SynchronizationContext
        {
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

            public override void Post(SendOrPostCallback callback, object state)
            {
                if (callback == null) throw new ArgumentNullException(nameof(callback));
                _queue.Add(new KeyValuePair<SendOrPostCallback, object>(callback, state));
            }

            public override void Send(SendOrPostCallback d, object state) => throw new NotSupportedException();

            public void RunOnCurrentThread()
            {
                foreach (var workItem in _queue.GetConsumingEnumerable())
                {
                    workItem.Key(workItem.Value);
                }
            }

            public void Complete() => _queue.CompleteAdding();
        }
    }
}
