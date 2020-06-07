using System;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class ReplyingTo : Event<Entry>
    {
        public ReplyingTo() {  }

        public string ReplyTo { get; set; }

        public ReplyingTo(string url, Guid entryId)
        {
            AggregateId = entryId;
            ReplyTo = url;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            aggregate.ReplyTo = ReplyTo;
        }
    }
}
