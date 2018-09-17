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

        public IReactiveSource<TRx> AddReactiveSource<TRx>(Enum sourceId)
        {
            return AddReactiveSource<TRx>(sourceId.ParseInt());
        }

        public IReactiveSource<TRx> AddReactiveSource<TRx>(int sourceId)
        {
            var source = ObservableBase<TRx>.CreateObservable();
            AddReactiveSource(sourceId, source);
            return source;
        }

        public void AddReactiveSource<TRx>(Enum sourceId, IObservable<TRx> source)
        {
            AddReactiveSource(sourceId.ParseInt(), source);
        }

        public void AddReactiveSource<TRx>(int sourceId, IObservable<TRx> source)
        {
            Repeater repeater = GetParticularRepeater<TRx>(sourceId);
            source.Subscribe((IObserver<TRx>)repeater.InnerSubject);
        }

        public IDisposable AddReactiveTarget<TRx>(Enum sourceId, Action<TRx> targetAction)
        {
            return AddReactiveTarget(sourceId.ParseInt(), targetAction);
        }

        public IDisposable AddReactiveTarget<TRx>(int sourceId, Action<TRx> targetAction)
        {
            var target = ObserverBase<TRx>.CreateObserver(targetAction);
            return AddReactiveTarget(sourceId, target);
        }

        public IDisposable AddReactiveTarget<TRx>(Enum sourceId, IObserver<TRx> target)
        {
            return AddReactiveTarget(sourceId.ParseInt(), target);
        }

        public IDisposable AddReactiveTarget<TRx>(int sourceId, IObserver<TRx> target)
        {
            Repeater repeater = GetParticularRepeater<TRx>(sourceId);
            return repeater.Subscribe(target);
        }

        public ReactionRouter()
        {
            //subscriptionを終了することを今は考えていない。
            //今後機能を持たせるかもしれない。
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
            internal IDisposable Subscribe(object observer) => CallSubscribe?.DynamicInvoke(observer) as IDisposable;

            internal object InnerSubject { get; }

            private Type genericType;
            private Type subjectTypeInfo;
            private object subjectInstance;

            private delegate IDisposable SubscribeDelegate<TRx>(IObserver<TRx> observer);
            private Delegate CallSubscribe;

            internal Repeater(Type subjectType)
            {
                var subjectInfo = CreateSubjectInstance(subjectType);
                MakeSubjectDelegate((subjectInfo.typeInfo, subjectInfo.instance, subjectType));

                this.genericType = subjectType;
                this.subjectTypeInfo = subjectInfo.typeInfo;
                this.subjectInstance = subjectInfo.instance;
                this.InnerSubject = this.subjectInstance; //IObservableにキャストしたい
            }

            private (Type typeInfo, object instance) CreateSubjectInstance(Type subjectType)
            {
                var genericSubjectType = typeof(Subject<>).MakeGenericType(subjectType);
                var subject = Activator.CreateInstance(genericSubjectType);
                return (genericSubjectType, subject);
            }

            private void MakeSubjectDelegate((Type typeInfo, object instance, Type subjectType) subjectInfo)
            {
                var subscribeMethod = subjectInfo.typeInfo.GetMethod("Subscribe");

                var genericDelegateType = typeof(SubscribeDelegate<>).MakeGenericType(subjectInfo.subjectType);

                CallSubscribe = Delegate.CreateDelegate(genericDelegateType, subjectInfo.instance, subscribeMethod);
            }
        }
    }
}
