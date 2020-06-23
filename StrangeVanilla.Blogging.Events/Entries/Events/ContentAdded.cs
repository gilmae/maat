using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class ContentAdded : Event<Entry>
    {

        public string[] Title { get; set; }
        public KeyValuePair<ContentType, string>[] Body { get; set; }
        public string BookmarkOf { get; set; }

        public ContentAdded() { }
        public ContentAdded(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);

            if (Body != null)
            {
                aggregate.Body = aggregate.Body.Concat(Body).ToArray();
            }

            if (Title != null)
            {
                aggregate.Title = aggregate.Title.Concat(Title).ToArray();
            }

            if (BookmarkOf != null)
            {
                aggregate.BookmarkOf = BookmarkOf;
            }
        }
    }
}
