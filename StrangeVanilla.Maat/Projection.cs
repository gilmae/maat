using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Events;
using libEnbilulu;

namespace StrangeVanilla.Maat
{
    public class Projection<T> : IProjection<T> where T : Aggregate
    {
        ConcurrentDictionary<Guid, T> projections = new ConcurrentDictionary<Guid, T>();

        public Projection(IEventStore<T> _aggregateRepository)
        {

            var events = _aggregateRepository.Retrieve().GroupBy(e => e.AggregateId);

            foreach (IGrouping<Guid, Event<T>> a in events)
            {
                T aggregate = (T)Activator.CreateInstance(typeof(T), a.Key);
                foreach (Event<T> e in a)
                {
                    aggregate = e.Apply(aggregate);
                }

                projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);
            }

            Task.Run(() =>
            {
                var _client = new Client(Environment.GetEnvironmentVariable("ENBILULUHOST"), int.Parse(Environment.GetEnvironmentVariable("ENBILULUPORT")));
                var streamName = typeof(T).Name;
                var lastPoint = 0;
                var stream = _client.GetStream(streamName);
                if (stream == null)
                {
                    stream = _client.CreateStream(streamName);
                }
                if (stream.Points == 0)
                {
                    lastPoint = stream.Last_Point;
                }
                else
                {
                    lastPoint = stream.Last_Point + 1;
                }

                while (true)
                {
                    var records = _client.GetRecordsFrom(streamName, lastPoint, 10);

                    var aggregateIds = records.Records.Select(p => System.Text.Json.JsonSerializer.Deserialize<Guid>(p.Payload)).Distinct();

                    if (aggregateIds.Count() > 0)
                    {
                        foreach (var id in aggregateIds)
                        {

                            T aggregate = (T)Activator.CreateInstance(typeof(T), id);
                            var events = _aggregateRepository.Retrieve(id);
                            foreach (var e in events)
                            {
                                aggregate = e.Apply(aggregate);
                            }
                            projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);

                        }
                    }
                    if (records.Records.Count() == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                    else
                    {
                        lastPoint = records.LastPoint.Value + 1;
                    }
                }
            });
        }

        public T Get(Guid id)
        {
            return projections[id];
        }

        public IEnumerable<T> Get()
        {
            return projections.Values;
        }


    }
}
