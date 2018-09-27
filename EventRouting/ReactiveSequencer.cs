using System;
using System.Reactive;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EventRouting
{
    public class ReactiveSequencer<TQueue>
    {
        private ConcurrentQueue<TQueue> normalQueue = new ConcurrentQueue<TQueue>();
        private ConcurrentQueue<TQueue> priorQueue = new ConcurrentQueue<TQueue>();

        private ReactiveProcessorBase<TQueue> processor;

        private object _lock_ = new object();

        public static ReactiveProcessorBase<TQueue> CreateInstance(Action<TQueue> action) => ReactiveProcessorBase<TQueue>.CreateInstance(action);

        public ReactiveSequencer(ReactiveProcessorBase<TQueue> processor)
        {
            this.processor = processor;
            processor.Exit.Subscribe(async () => await Exited());
        }

        public async void Enqueue(TQueue newParameter)
        {
            lock (_lock_)
            {
                normalQueue.Enqueue(newParameter);
            }
            await Entried();
        }

        public async void PrioritizedEnqueue(TQueue newParameter)
        {
            lock (_lock_)
            {
                priorQueue.Enqueue(newParameter);
            }
            await Entried();
        }

        private async Task Entried()
        {
            int count;
            lock (_lock_)
            {
                count = normalQueue.Count + priorQueue.Count;
            }

            if (count == 1) await DoExecute();
        }

        private async Task Exited()
        {
            await DoExecute();
        }

        private Task DoExecute()
        {
            TQueue parameter;
            lock (_lock_)
            {
                if (!priorQueue.TryDequeue(out parameter))
                {
                    if (!normalQueue.TryDequeue(out parameter))
                    {
                        return Task.CompletedTask;
                    }
                }
            }

            return Task.Run(() => {
                this.processor.Run(parameter);
            });
        }
    }
}
