using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryPublished : Event<Entry>
    {
        public EntryPublished() { }
        public EntryPublished(Guid entryId) : this(entryId, DateTime.UtcNow)
        {
            
        }

        public EntryPublished(Guid entryId, DateTime publishedAt)
        {
            AggregateId = entryId;
            PublishedAt = publishedAt;
        }

        public DateTime PublishedAt { get; set; }


        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.PublishedAt = PublishedAt;
        }
    }
}
