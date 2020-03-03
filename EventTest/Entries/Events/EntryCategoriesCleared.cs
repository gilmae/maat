using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryCategoriesCleared : Event<Entry>
    {
        public EntryCategoriesCleared(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry entry)
        {
            base.Apply(entry);
            entry.Categories = new List<string>();
        }
    }
}
