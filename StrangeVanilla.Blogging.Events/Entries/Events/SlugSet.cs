using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class SlugSet : Event<Entry>
    {
        public string Slug { get; set; }
        public SlugSet() { }
        public SlugSet(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.Slug = Slug;
        }
    }
}
