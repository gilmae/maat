using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryPublished : Event<Entry>
    {
        public EntryPublished() { }
        public EntryPublished(Entry entry) : this(entry, DateTime.UtcNow)
        {
            
        }

        public EntryPublished(Entry entry, DateTime publishedAt)
        {
            AggregateId = entry.Id;
            Published_At = publishedAt;
        }

        public DateTime Published_At { get; set; }


        public override Entry Apply(Entry aggregate)
        {
            aggregate.Published_At = Published_At;
            return aggregate;
        }
    }
}
