using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Events;
using StrangeVanilla.Blogging.Events;
using mfv = SV.Maat.Micropub.Models;
using Microsoft.Extensions.Logging;
using mf;

namespace SV.Maat.Projections
{
    public class EntryProjection : ProjectionBase, IEntryProjection
    {
        ConcurrentDictionary<Guid, Post> projections = new ConcurrentDictionary<Guid, Post>();
        public EntryProjection(ILogger<EntryProjection> logger, IEventStore<Post> eventStore) : base(logger, eventStore)
        {
        }

        public mfv.Entry Get(Guid id)
        {
            if (projections.TryGetValue(id, out Post entry))
            {
                return entry.Data.AsVocab<mfv.Entry>();
            }
            return null;
        }

        public mfv.Entry Get(string slug)
        {
            return projections.Values.FirstOrDefault(e => e.Data.Properties.ContainsKey("slug")
                && e.Data.Properties["slug"].Any(p => p as string == slug))?.Data.AsVocab<mfv.Entry>();
        }

        public IEnumerable<mfv.Entry> Get()
        {
            return projections.Values.OrderByDescending(e => e.CreatedAt).Select(p=>p.Data.AsVocab<mfv.Entry>());
        }

        public IEnumerable<mfv.Entry> GetAfter(int numItems, DateTime after)
        {
            return projections.Values.Where(x => x.CreatedAt > after).Take(numItems).Select(p => p.Data.AsVocab<mfv.Entry>());
        }

        public IEnumerable<mfv.Entry> GetBefore(int numItems, DateTime before)
        {
            return projections.Values.Where(x => x.CreatedAt < before).Take(numItems).Select(p => p.Data.AsVocab<mfv.Entry>());
        }

        public IEnumerable<mfv.Entry> GetLatest(int numItems)
        {
            return Get().Take(numItems);
        }

        public override (int?, int) ProcessEvents(IList<Event> newEvents)
        {
            if (newEvents.Any())
            {
                var entryEvents = newEvents.Where(e => e is Event<Post>);
                logger.LogTrace("Processing {0} new potential entry events", entryEvents.Count());
                foreach (Event<Entry> e in entryEvents.Where(e => e is Event<Entry>))
                {
                    if (!projections.TryGetValue(e.AggregateId, out Post aggregate))
                    {
                        aggregate = new Post(e.AggregateId);
                    }
                    e.Apply(aggregate);

                    if (aggregate.Data.Type.Contains("h-entry"))
                    {
                        projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);
                    }
                }
                int maxId = newEvents.Max(e => e.Id);
                logger.LogTrace("Entry projection updated, last id processed: {0}", maxId);
                return (maxId, 0);
            }

            return (null, 5);
        }
    }
}
