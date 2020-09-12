using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.Projections
{
    public class EntryProjection : ProjectionBase, IEntryProjection
    {
        ConcurrentDictionary<Guid, Entry> projections = new ConcurrentDictionary<Guid, Entry>();
        public EntryProjection(ILogger<EntryProjection> logger, IEventStore<Entry> eventStore) : base(logger, eventStore)
        {
        }

        public Entry Get(Guid id)
        {
            if (projections.TryGetValue(id, out Entry entry))
            {
                return entry;
            }
            return null;
        }

        public Entry Get(string slug)
        {
            return projections.Values.FirstOrDefault(e => e.Slug == slug);
        }

        public IEnumerable<Entry> Get()
        {
            return projections.Values.OrderByDescending(e => e.CreatedAt);
        }

        public IEnumerable<Entry> GetAfter(int numItems, DateTime after)
        {
            return Get().Where(x => x.CreatedAt > after).Take(numItems);
        }

        public IEnumerable<Entry> GetBefore(int numItems, DateTime before)
        {
            return Get().Where(x => x.CreatedAt < before).Take(numItems);
        }

        public IEnumerable<Entry> GetLatest(int numItems)
        {
            return Get().Take(numItems);
        }

        public override (int?, int) ProcessEvents(IList<Event> newEvents)
        {
            if (newEvents.Any())
            {
                var entryEvents = newEvents.Where(e => e is Event<Entry>);
                logger.LogTrace("Processing {0} new Entry events", entryEvents.Count());
                foreach (Event<Entry> e in entryEvents.Where(e => e is Event<Entry>))
                {
                    if (!projections.TryGetValue(e.AggregateId, out Entry aggregate))
                    {
                        aggregate = new Entry(e.AggregateId);
                    }
                    e.Apply(aggregate);
                    projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);
                }
                int maxId = newEvents.Max(e => e.Id);
                logger.LogTrace("Entry projection updated, last id processed: {0}", maxId);
                return (maxId, 0);
            }

            return (null, 5);
        }
    }
}
