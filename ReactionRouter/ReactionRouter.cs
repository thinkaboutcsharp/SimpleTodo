using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxRouting
{
    public class ReactionRouter
    {
        private Dictionary<int, Repeater> repeaters = new Dictionary<int, Repeater>();

        public void AddReactiveSource<TRx>(int sourceId, IObservable<TRx> source)
        {
            var observableDelegate = (Func<IObserver<TRx>, IDisposable>)(source.Subscribe);
            var observable = Observable.Create(observableDelegate);

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
            public IDisposable Subscribe(object observer) => CallSubscribe?.DynamicInvoke(observer) as IDisposable;

            public object InnerSubject => subscribeInstance;

            private Type genericType;
            private Type subjectTypeInfo;
            private object subjectInstance;
            private object subscribeInstance;

            private delegate IDisposable SubscribeDelegate<TRx>(IObserver<TRx> observer);
            private Delegate CallSubscribe;

            public Repeater(Type subjectType)
            {
                var subjectInfo = CreateSubjectInstance(subjectType);
                MakeSubjectDelegate((subjectInfo.typeInfo, subjectInfo.instance, subjectType));

                this.genericType = subjectType;
                this.subjectTypeInfo = subjectInfo.typeInfo;
                this.subjectInstance = subjectInfo.instance;
                this.subscribeInstance = this.subjectInstance; //IObservableにキャストしたい
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
