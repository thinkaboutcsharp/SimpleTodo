using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace EventRouting
{
    public class ReactionRouter
    {
        private Dictionary<int, Repeater> repeaters = new Dictionary<int, Repeater>();

        public static ObservableBase<TRx> CreateReactiveSource<TRx>()
            => ObservableBase<TRx>.CreateObservable();

        public IReactiveSource<TRx> AddReactiveSource<TRx>(Enum sourceId) => AddReactiveSource<TRx>(sourceId.ParseInt());

        public IReactiveSource<TRx> AddReactiveSource<TRx>(int sourceId)
        {
            var source = ObservableBase<TRx>.CreateObservable();
            AddReactiveSource(sourceId, source);
            return source;
        }

        public void AddReactiveSource<TRx>(Enum sourceId, IObservable<TRx> source) => AddReactiveSource(sourceId.ParseInt(), source);

        public void AddReactiveSource<TRx>(int sourceId, IObservable<TRx> source)
        {
            var repeater = GetParticularRepeater<TRx>(sourceId);
            repeater.SubscribeSource(source);
        }

        public IDisposable AddReactiveTarget<TRx>(Enum sourceId, Action<TRx> targetAction) => AddReactiveTarget(sourceId.ParseInt(), targetAction);

        public IDisposable AddReactiveTarget<TRx>(int sourceId, Action<TRx> targetAction)
        {
            var target = ObserverBase<TRx>.CreateObserver(targetAction);
            return AddReactiveTarget(sourceId, target);
        }

        public IDisposable AddReactiveTarget<TRx>(Enum sourceId, IObserver<TRx> target) => AddReactiveTarget(sourceId.ParseInt(), target);

        public IDisposable AddReactiveTarget<TRx>(int sourceId, IObserver<TRx> target)
        {
            var repeater = GetParticularRepeater<TRx>(sourceId);
            var disposer = repeater.PublishToTarget(target);
            return disposer;
        }

        public void StopRouting<TRx>(Enum sourceId) => StopRouting<TRx>(sourceId.ParseInt());

        public void StopRouting<TRx>(int sourceId)
        {
            var repeater = GetParticularRepeater<TRx>(sourceId);
            repeater.StopRepeater<TRx>();
        }

        private Repeater GetParticularRepeater<TRx>(int sourceId)
        {
            Repeater repeater;

            if (repeaters.ContainsKey(sourceId))
            {
                repeater = repeaters[sourceId];
            }
            else
            {
                repeater = new Repeater(typeof(TRx));
                repeaters.Add(sourceId, repeater);
            }

            return repeater;
        }

        private class Repeater
        {
            internal object InnerSubject { get; }

            private Type subjectType;
            private System.Collections.IList sourceList;

            internal void SubscribeSource<TRx>(IObservable<TRx> observable)
            {
                if (typeof(TRx) != subjectType)
                {
                    throw new ArgumentException("Subjectの型がObservableの型と一致しません。[" + typeof(TRx) + "] is not [" + subjectType + "]");
                }
                var disposer = observable.Subscribe((IObserver<TRx>)InnerSubject);
                var source = new Source<TRx>(this, disposer);
                sourceList.Add(source);
            }

            internal IDisposable PublishToTarget<TRx>(IObserver<TRx> target)
            {
                if (typeof(TRx) != subjectType)
                {
                    throw new ArgumentException("Subjectの型がObserverの型と一致しません。[" + typeof(TRx) + "] is not [" + subjectType + "]");
                }
                var subjectDisposer = ((IObservable<TRx>)InnerSubject).Subscribe(target);
                return subjectDisposer;
            }

            internal void StopRepeater<TRx>()
            {
                if (typeof(TRx) != subjectType)
                {
                    throw new ArgumentException("Subjectの型が指定の型と一致しません。[" + typeof(TRx) + "] is not [" + subjectType + "]");
                }
                Source<TRx>[] sourceArray = new Source<TRx>[sourceList.Count];
                sourceList.CopyTo(sourceArray, 0);
                foreach (var source in sourceArray)
                {
                    var observable = (Source<TRx>)source;
                    observable.Dispose();
                }
                ((IObserver<TRx>)InnerSubject).OnCompleted();
            }

            private void Unsubscribe<TRx>(Source<TRx> source)
            {
                sourceList.Remove(source);
            }

            internal Repeater(Type subjectType)
            {
                this.subjectType = subjectType;

                var subject = CreateSubjectInstance();
                this.InnerSubject = subject;

                this.sourceList = CreateSourceList();
            }

            private object CreateSubjectInstance()
            {
                var genericSubjectType = typeof(Subject<>).MakeGenericType(subjectType);
                var subject = Activator.CreateInstance(genericSubjectType);
                return subject;
            }

            private System.Collections.IList CreateSourceList()
            {
                return CreateList(typeof(Source<>));
            }

            private System.Collections.IList CreateList(Type listType)
            {
                var genericSourceType = listType.MakeGenericType(subjectType);
                var genericListType = typeof(List<>).MakeGenericType(genericSourceType);
                var list = Activator.CreateInstance(genericListType);
                return (System.Collections.IList)list;
            }

            private class Source<TRx>
            {
                private Repeater parent;
                private IDisposable disposer;

                internal Source(Repeater parent, IDisposable disposer)
                {
                    this.parent = parent;
                    this.disposer = disposer;
                }

                internal void Dispose()
                {
                    disposer.Dispose();
                    parent.Unsubscribe(this);
                }
            }
        }
    }
}
