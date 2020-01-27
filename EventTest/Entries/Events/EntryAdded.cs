using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryAdded : Event<Entry>
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string BookmarkOf { get; set; }

        public EntryAdded() { }
        public EntryAdded(Entry entry)
        {
            AggregateId = entry.Id;
        }

        public override Entry Apply(Entry aggregate)
        {
            aggregate.Body = Body;
            aggregate.Title = Title;
            aggregate.BookmarkOf = BookmarkOf;
            return aggregate;
        }
    }
}
