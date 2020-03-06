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
            Published_At = publishedAt;
        }

        public DateTime Published_At { get; set; }


        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.Published_At = Published_At;
        }
    }
}
