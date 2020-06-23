using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class ContentSet : Event<Entry>
    {

        public string[] Title { get; set; }
        public KeyValuePair<ContentType, string>[] Body { get; set; }
        public string BookmarkOf { get; set; }

        public ContentSet() { }
        public ContentSet(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);

            if (Body != null) 
            {
                aggregate.Body = Body;
            }

            if (Title != null)
            {
                aggregate.Title = Title;
            }

            if (BookmarkOf != null)
            {
                aggregate.BookmarkOf = BookmarkOf;
            }
        }
    }
}
