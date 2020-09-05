using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class SyndicationPublished : Event<Entry>
    {
        public string Syndication { get; set; }
        public string Network { get; set; }

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
                aggregate.Syndications = new List<Entry.Syndication> { new Entry.Syndication { Url = Syndication, Network = Network } };
            }
            else
            {
                aggregate.Syndications.Add(new Entry.Syndication { Url = Syndication, Network = Network });
                aggregate.Syndications = aggregate.Syndications.Distinct().ToList();
            }
        }
    }
}
