using System;
using System.Collections.Generic;
using System.Linq;

namespace EventRouting
{
    public interface IRequestable<TRq>
    {
        TRq Request(object param);
    }

    public interface IRequester<TRq>
    {
        IEnumerable<TRq> Request(object param = null);
        TRq RequestSingle(object param = null);
        TResult RequestAggregate<TResult>(Func<IEnumerable<TRq>, TResult> func, object param = null);
    }

    public class RequestRouter
    {
        private Dictionary<int, Repeater> repeaters = new Dictionary<int, Repeater>();

        public void AddRequestable<TRq>(Enum sourceId, Func<object, TRq> requestableFunc)
        {
            AddRequestable(sourceId.ParseInt(), requestableFunc);
        }

        public void AddRequestable<TRq>(int sourceId, Func<object, TRq> requestableFunc)
        {
            var requestable = new RequestableImpl<TRq>(requestableFunc);
            AddRequestable(sourceId, requestable);
        }

        public void AddRequestable<TRq>(Enum sourceId, IRequestable<TRq> requestable)
        {
            AddRequestable(sourceId.ParseInt(), requestable);
        }

        public void AddRequestable<TRq>(int sourceId, IRequestable<TRq> requestable)
        {
            var repeater = GetParticularRepeater<TRq>(sourceId);
            repeater.Add(requestable);
        }

        public IRequester<TRq> CreateRequester<TRq>(Enum sourceId)
        {
            return CreateRequester<TRq>(sourceId.ParseInt());
        }

        public IRequester<TRq> CreateRequester<TRq>(int sourceId)
        {
            var requester = new RequesterImpl<TRq>(GetParticularRepeater<TRq>(sourceId));
            return requester;
        }

        public Action GetAssignAction<TRq>(int sourceId, Action<TRq> assignAction)
        {
            return GetAssignAction(sourceId, (object)null, assignAction);
        }

        public Action GetAssignAction<TRq>(int sourceId, Func<IEnumerable<TRq>, TRq> aggregateFunc, Action<TRq> assignAction)
        {
            return GetAssignAction(sourceId, null, aggregateFunc, assignAction);
        }

        public Action GetAssignAction<TRq>(int sourceId, object param, Action<TRq> assignAction)
        {
            return GetAssignAction(sourceId, param, (IEnumerable<TRq> results) => results.FirstOrDefault(), assignAction);
        }

        public Action GetAssignAction<TRq, TResult>(int sourceId, object param, Func<IEnumerable<TRq>, TResult> aggregateFunc, Action<TResult> assignAction)
        {
            return () =>
            {
                if (repeaters.ContainsKey(sourceId))
                {
                    var repeater = repeaters[sourceId];
                    var value = aggregateFunc(repeater.Request<TRq>(param));
                    assignAction(value);
                }
            };
        }

        private Repeater GetParticularRepeater<TRq>(int sourceId)
        {
            Repeater repeater;
            if (repeaters.ContainsKey(sourceId))
            {
                repeater = repeaters[sourceId];
            }
            else
            {
                repeater = new Repeater(typeof(TRq));
                repeaters.Add(sourceId, repeater);
            }

            return repeater;
        }

        private class RequestableImpl<TRq> : IRequestable<TRq>
        {
            private Func<object, TRq> func;

            internal RequestableImpl(Func<object, TRq> func) => this.func = func;

            public TRq Request(object param = null) => func(param);
        }

        private class RequesterImpl<TRq> : IRequester<TRq>
        {
            private Repeater repeater;

            internal RequesterImpl(Repeater repeater) => this.repeater = repeater;

            public IEnumerable<TRq> Request(object param = null) => repeater.Request<TRq>(param);

            public TRq RequestSingle(object param = null) => repeater.RequestSingle<TRq>(param);

            public TResult RequestAggregate<TResult>(Func<IEnumerable<TRq>, TResult> func, object param = null)
            {
                var requested = repeater.Request<TRq>(param);
                var result = func(requested);
                return result;
            }
        }

        private class Repeater
        {
            private Type requestType;
            private object requestables;

            internal Repeater(Type requestType)
            {
                this.requestType = requestType;

                var repeaterType = typeof(IRequestable<>).MakeGenericType(requestType);
                var requestableType = typeof(List<>).MakeGenericType(repeaterType);
                requestables = Activator.CreateInstance(requestableType);
            }

            internal void Add<TRq>(IRequestable<TRq> requestable) => ((List<IRequestable<TRq>>)requestables).Add(requestable);

            internal IEnumerable<TRq> Request<TRq>(object param)
            {
                var results = new List<TRq>();
                foreach (var requestable in (List<IRequestable<TRq>>)requestables)
                {
                    results.Add(requestable.Request(param));
                }

                return results.AsEnumerable();
            }

            internal TRq RequestSingle<TRq>(object param)
            {
                var requestableList = (List<IRequestable<TRq>>)requestables;
                return requestableList.Count > 0 ? requestableList[0].Request(param) : default;
            }
        }
    }
}
