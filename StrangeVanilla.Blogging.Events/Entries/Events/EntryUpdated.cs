using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class EntryUpdated : Event<Entry>
    {

        public string Title { get; set; }
        public string Body { get; set; }
        public string BookmarkOf { get; set; }
        public string ReplyTo { get; set; }

        public EntryUpdated() { }
        public EntryUpdated(Guid entryId)
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

            if (ReplyTo != null)
            {
                aggregate.ReplyTo = ReplyTo;
            }
        }
    }
}
