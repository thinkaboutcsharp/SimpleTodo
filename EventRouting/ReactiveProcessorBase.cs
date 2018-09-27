using System;
using Reactive.Bindings;

namespace EventRouting
{
    public abstract class ReactiveProcessorBase<TQueue>
    {
        public ReactiveCommand<ReactiveProcessorException> ExceptionOccured = new ReactiveCommand<ReactiveProcessorException>();

        protected abstract void Execute(TQueue parameter);

        internal ReactiveCommand Exit = new ReactiveCommand();

        internal static ReactiveProcessorBase<TQueue> CreateInstance(Action<TQueue> action) => new ReactiveProcessorBaseImpl(action);

        protected ReactiveProcessorBase()
        {
        }

        internal void Run(TQueue parameter)
        {
            try
            {
                Execute(parameter);
            }
            catch (Exception ex)
            {
                var capsule = new ReactiveProcessorException(ex, System.Threading.Thread.CurrentThread.ManagedThreadId, parameter);
                ExceptionOccured.Execute(capsule);
            }

            Exit.Execute();
        }

        private class ReactiveProcessorBaseImpl : ReactiveProcessorBase<TQueue>
        {
            private Action<TQueue> action;
            internal ReactiveProcessorBaseImpl(Action<TQueue> action)
            {
                this.action = action;
            }

            protected override void Execute(TQueue parameter)
            {
                action(parameter);
            }
        }
    }
}
