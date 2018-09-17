using System;

namespace EventRouting
{
    public interface IReactiveSource<T>
    {
        void Send(T parameter);
    }

    public abstract class ObservableBase<TRx> : IReactiveSource<TRx>, IObservable<TRx>
    {
        protected IObserver<TRx> observer;

        public static ObservableBase<TRx> CreateObservable() => new ObservableInternalImpl();

        public virtual IDisposable Subscribe(IObserver<TRx> observer)
        {
            this.observer = observer;
            return System.Reactive.Disposables.Disposable.Empty;
        }

        public virtual void Send(TRx parameter)
        {
            observer.OnNext(parameter);
        }

        private class ObservableInternalImpl : ObservableBase<TRx>
        {}
    }
}
