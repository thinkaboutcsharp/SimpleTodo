using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace RxRouting
{
    public class ReactionRouter
    {
        private Dictionary<int, Repeater> repeaters = new Dictionary<int, Repeater>();

        public void AddReactiveSource<TRx>(int sourceId, IObservable<TRx> source)
        {
            Repeater repeater = GetParticularRepeater<TRx>(sourceId);
            source.Subscribe((IObserver<TRx>)repeater.InnerSubject);
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

            if (!repeaters.ContainsKey(sourceId))
            {
                repeater = new Repeater(typeof(TRx));
                repeaters.Add(sourceId, repeater);
            }
            else
            {
                repeater = repeaters[sourceId];
            }

            return repeater;
        }

        class Repeater
        {
            public IDisposable Subscribe(object observer) => CallSubscribe?.Invoke(observer);

            public object InnerSubject => subscribeInstance;

            private Type genericType;
            private Type subjectTypeInfo;
            private object subjectInstance;
            private object subscribeInstance;

            private delegate IDisposable SubscribeDelegate(object observer);
            private SubscribeDelegate CallSubscribe;

            public Repeater(Type subjectType)
            {
                var subjectInfo = CreateSubjectInstance(subjectType);
                MakeSubjectDelegate(subjectInfo);

                this.genericType = subjectType;
                this.subjectTypeInfo = subjectInfo.typeInfo;
                this.subjectInstance = subjectInfo.instance;
                this.subscribeInstance = CastToObserver();
            }

            private (Type typeInfo, object instance) CreateSubjectInstance(Type subjectType)
            {
                var subjectTypeInfo = typeof(Subject<>).MakeGenericType(subjectType);
                var subjectInstance = Activator.CreateInstance(subjectTypeInfo);
                return (subjectTypeInfo, subjectInstance);
            }

            private void MakeSubjectDelegate((Type typeInfo, object instance) subjectInfo)
            {
                var subscribeMethod = subjectInfo.typeInfo.GetMethod("Subscribe");

                CallSubscribe = Delegate.CreateDelegate(typeof(IDisposable), subscribeMethod) as SubscribeDelegate;
            }

            private object CastToObserver()
            {
                var interfaceType = typeof(IObserver<>).MakeGenericType(this.genericType);
                var observer = Convert.ChangeType(this.subjectInstance, interfaceType);
                return observer;
            }
        }
    }
}
