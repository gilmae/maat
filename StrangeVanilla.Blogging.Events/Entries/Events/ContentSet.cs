using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    [Obsolete]
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
                Content content = new Content();
                content.Value = Body.First((k) => k.Key == ContentType.plaintext).Value;
                if (Body.Any(kv => kv.Key != ContentType.plaintext))
                {
                    var k = Body.FirstOrDefault(kv => kv.Key != ContentType.plaintext);
                    content.Type = k.Key;
                    content.Markup = k.Value;
                }

                aggregate.Body = content;
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
