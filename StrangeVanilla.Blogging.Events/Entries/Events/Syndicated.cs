using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class Syndicated : Event<Entry>
    {
        public Syndicated() { }
        public Syndicated(Guid entryId, string syndicationAccount)
        {
            AggregateId = entryId;
            SyndicationAccount = syndicationAccount;
        }

        public string SyndicationAccount { get; set; }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            if (aggregate.SyndicateTo == null)
            {
                aggregate.SyndicateTo = new List<string> { SyndicationAccount };
            }
            else
            {
                aggregate.SyndicateTo.Add(SyndicationAccount);
                aggregate.SyndicateTo = aggregate.SyndicateTo.Distinct().ToList();
            }
        }
    }
}
