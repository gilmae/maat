using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryCategorised : Event<Entry>
    {
        public string Category { get; set; }

        public EntryCategorised() { }
        public EntryCategorised(Entry entry, string category)
        {
            AggregateId = entry.Id;
            Category = category;
        }

        public override Entry Apply(Entry aggregate)
        {
            if (aggregate.Categories == null)
            {
                aggregate.Categories = new[] { Category };
            }
            else
            {
                aggregate.Categories.Add(Category);
            }

            return aggregate;
        }
    }
}
