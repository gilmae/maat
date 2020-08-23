using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class OwnerSet : Event<Entry>
    {
        public int OwnerId { get; set; }
        public OwnerSet() { }
        public OwnerSet(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.OwnerId = OwnerId;
        }
    }
}
