using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryDeleted : Event<Entry>
    {
        public EntryDeleted()
        {
        }

        public DateTime DeletedAt { get; set; }

        public EntryDeleted(Guid entryId)
        {
            AggregateId = entryId;
            DeletedAt = DateTime.UtcNow;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.DeletedAt = DeletedAt;
        }
    }
}
