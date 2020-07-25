using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Projections
{
    public class SyndicationProjection : ProjectionBase, ISyndicationsProjection
    {
        public SyndicationProjection(ILogger<SyndicationProjection> logger, IEventStore<Entry> eventStore) : base(logger, eventStore) { }

        private ConcurrentDictionary<string, Guid> syndications = new ConcurrentDictionary<string, Guid>();
        public Guid? GetEntryForSyndication(string url)
        {
            if (syndications.TryGetValue(url, out Guid entryId))
            {
                return entryId;
            }
            return null;
        }

        public override (int?, int) ProcessEvents(IList<Event> newEvents)
        {
            if (newEvents.Any())
            {
                var entryEvents = newEvents.Where(e => e is Event<Entry>);
                logger.LogTrace("Processing {0} new Entry events", entryEvents.Count());
                foreach (SyndicationPublished e in entryEvents.Where(e => e is SyndicationPublished))
                {
                    syndications.AddOrUpdate(e.Syndication, e.AggregateId, (key, aggregateId) => e.AggregateId);
                }
                int maxId = newEvents.Max(e => e.Id);
                logger.LogTrace("Replies projection updated, last id processed: {0}", maxId);
                return (maxId, 0);
            }
            return (null, 5);
        }
    }
}
