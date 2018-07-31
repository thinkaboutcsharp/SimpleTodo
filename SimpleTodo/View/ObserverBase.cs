using System;
using Xamarin.Forms;

namespace SimpleTodo
{
    public class ObserverBase<TRx> : IObserver<TRx>
    {
        protected Action<TRx> handler;

        public ObserverBase(Action<TRx> action)
        {
            handler = action;
        }

        public virtual void OnCompleted() { }

        public virtual void OnError(Exception error) { }

        public virtual void OnNext(TRx value)
        {
            handler(value);
        }
    }
}
