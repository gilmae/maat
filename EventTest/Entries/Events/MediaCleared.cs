using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class MediaCleared : Event<Entry>
    {
        public MediaCleared(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry entry)
        {
            base.Apply(entry);
            entry.AssociatedMedia = new List<string>();
        }
    }
}
