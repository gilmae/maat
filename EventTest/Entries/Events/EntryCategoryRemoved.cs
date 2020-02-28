using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryCategoryRemoved : Event<Entry>
    {
        public string Category { get; set; }

        public EntryCategoryRemoved(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            var index = aggregate.Categories?.IndexOf(Category);
            if (index.HasValue && index.Value != -1)
            {
                aggregate.Categories.RemoveAt(index.Value);
            }
        }
    }
}
