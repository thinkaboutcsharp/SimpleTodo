using System;

namespace EventRouting
{
    public abstract class ObserverBase<TRx> : IObserver<TRx>
    {
        protected Action<TRx> handler;

        protected ObserverBase(Action<TRx> action)
        {
            handler = action;
        }

        public static IObserver<TRx> CreateObserver(Action<TRx> action) => new ObserverInternalImpl(action);

        public virtual void OnCompleted() { }

        public virtual void OnError(Exception error) { }

        public virtual void OnNext(TRx value)
        {
            handler(value);
        }

        private class ObserverInternalImpl : ObserverBase<TRx>
        {
            internal ObserverInternalImpl(Action<TRx> action) : base(action){}
        }
    }
}
