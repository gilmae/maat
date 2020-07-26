using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class ContentAdded2 : Event<Entry>
    {

        public string[] Title { get; set; }
        public Content Body { get; set; }
        public string BookmarkOf { get; set; }

        public ContentAdded2() { }
        public ContentAdded2(Guid entryId)
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
                aggregate.Title = aggregate.Title.Concat(Title).ToArray();
            }

            if (BookmarkOf != null)
            {
                aggregate.BookmarkOf = BookmarkOf;
            }
        }
    }
}
