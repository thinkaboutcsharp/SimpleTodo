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
    }

    public class RequestRouter
    {
        private Dictionary<int, Repeater> repeaters = new Dictionary<int, Repeater>();

        public void AddRequestable<TRq>(Enum sourceId, IRequestable<TRq> requestable)
        {
            string name = Enum.GetName(sourceId.GetType(), sourceId);
            int value = (int)Enum.Parse(sourceId.GetType(), name);
            AddRequestable(value, requestable);
        }

        public void AddRequestable<TRq>(int sourceId, IRequestable<TRq> requestable)
        {
            var repeater = GetParticularRepeater<TRq>(sourceId);
            repeater.Add(requestable);
        }

        public IRequester<TRq> GetRequester<TRq>(Enum sourceId)
        {
            string name = Enum.GetName(sourceId.GetType(), sourceId);
            int value = (int)Enum.Parse(sourceId.GetType(), name);
            return GetRequester<TRq>(value);
        }

        public IRequester<TRq> GetRequester<TRq>(int sourceId)
        {
            var requester = new RequesterImpl<TRq>(GetParticularRepeater<TRq>(sourceId));
            return requester;
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

        class RequesterImpl<TRq> : IRequester<TRq>
        {
            private Repeater repeater;

            internal RequesterImpl(Repeater repeater)
            {
                this.repeater = repeater;
            }

            public IEnumerable<TRq> Request(object param = null)
            {
                return repeater.Request<TRq>(param);
            }

            public TRq RequestSingle(object param = null)
            {
                return repeater.RequestSingle<TRq>(param);
            }
        }

        class Repeater
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
