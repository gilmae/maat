using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Maat
{
    public class Projection<T> : IProjection<T> where T : Aggregate
    {
        //Client _client;

        Dictionary<Guid, T> projections = new Dictionary<Guid, T>();

        public Projection(IEventStore<T> _aggregateRepository)
        {
            //_client = new Client(Environment.GetEnvironmentVariable("MAATCONNSTR"), 6700);

            var events = _aggregateRepository.Retrieve().GroupBy(e => e.AggregateId);

            foreach (IGrouping<Guid, Event<T>> a in events)
            {
                T aggregate = (T)Activator.CreateInstance(typeof(T), a.Key);
                foreach (Event<T> e in a)
                {
                    aggregate = e.Apply(aggregate);
                }

                projections.Add(aggregate.Id, aggregate);
            }
        }

        public T Get(Guid id)
        {
            return projections[id];
        }
    }
}
