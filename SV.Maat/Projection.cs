using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Events;
using libEnbilulu;
using StrangeVanilla.Blogging.Events;
using SV.Maat.lib.MessageBus;

namespace SV.Maat
{
    public class EntryProjection : IProjection<Entry, Guid>
    {
        ConcurrentDictionary<Guid, Entry> projections = new ConcurrentDictionary<Guid, Entry>();

        public EntryProjection(IEventStore<Entry> _aggregateRepository)
        {

            var events = _aggregateRepository.Retrieve().GroupBy(e => e.AggregateId);

            foreach (IGrouping<Guid, Event<Entry>> a in events)
            {
                Entry aggregate = (Entry)Activator.CreateInstance(typeof(Entry), a.Key);
                foreach (Event<Entry> e in a)
                {
                    e.Apply(aggregate);
                }

                projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);
            }

            Task.Run(() =>
            {
                var _client = new Client(Environment.GetEnvironmentVariable("ENBILULUHOST"), int.Parse(Environment.GetEnvironmentVariable("ENBILULUPORT")));
                var streamName = typeof(Entry).Name;
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

                    if (records.Records.Count() == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                    else
                    {
                        var payloadData = records.Records.Select(p => System.Text.Json.JsonSerializer.Deserialize<AggregateEventMessage>(p.Payload)).ToList();
                        var aggregateIds = payloadData.Select(i => i.Id).Distinct();

                        if (aggregateIds.Count() > 0)
                        {
                            foreach (var id in aggregateIds)
                            {

                                Entry aggregate = (Entry)Activator.CreateInstance(typeof(Entry), id);
                                var events = _aggregateRepository.Retrieve(id);
                                foreach (var e in events)
                                {
                                    e.Apply(aggregate);
                                }
                                projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);

                            }
                        }

                        lastPoint = records.LastPoint.Value + 1;
                    }
                }
            });
        }

        public Entry Get(Guid id)
        {
            return projections[id];
        }

        public IEnumerable<Entry> Get()
        {
            return projections.Values;
        }


    }
}
