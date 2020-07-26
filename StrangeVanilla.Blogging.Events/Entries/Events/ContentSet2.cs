using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class ContentSet2 : Event<Entry>
    {

        public string[] Title { get; set; }
        public Content Body { get; set; }
        public string BookmarkOf { get; set; }

        public ContentSet2() { }
        public ContentSet2(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);

            aggregate.Body = Body;

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
