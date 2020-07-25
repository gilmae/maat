using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryUndeleted : Event<Entry>
    {
        public EntryUndeleted()
        {
        }

        public EntryUndeleted(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.DeletedAt = null;
        }
    }
}
