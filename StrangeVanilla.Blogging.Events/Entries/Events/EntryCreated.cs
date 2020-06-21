using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryCreated : Event<Entry>
    {
        public DateTime CreatedAt { get; set; }

        public EntryCreated() { CreatedAt = DateTime.UtcNow; }
        public EntryCreated(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.CreatedAt = CreatedAt;
        }
    }
}

