using System;
namespace SimpleTodo
{
    public abstract class ObservableBase<TRx> : IObservable<TRx>
    {
        protected IObserver<TRx> observer;

        public virtual IDisposable Subscribe(IObserver<TRx> observer)
        {
            this.observer = observer;
            return System.Reactive.Disposables.Disposable.Empty;
        }

        public virtual void Send(TRx parameter)
        {
            observer.OnNext(parameter);
        }
    }
}
