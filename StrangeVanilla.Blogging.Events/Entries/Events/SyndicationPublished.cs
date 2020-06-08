using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class SyndicationPublished : Event<Entry>
    {
        public string Syndication { get; set; }

        public SyndicationPublished(){}

        public SyndicationPublished(Guid aggregateId, string syndication)
        {
            AggregateId = aggregateId;
            Syndication = syndication;
        }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            if (aggregate.Syndications == null)
            {
                aggregate.Syndications = new List<string> { Syndication };
            }
            else
            {
                aggregate.Syndications.Add(Syndication);
                aggregate.Syndications = aggregate.Syndications.Distinct().ToList();
            }
        }
    }
}
