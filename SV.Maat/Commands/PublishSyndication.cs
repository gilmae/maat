using System;
using System.Linq;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class PublishSyndication : ICommand
    {
        public PublishSyndication()
        {
        }

        public string SyndicationUrl { get; set; }
        public string Network { get; set; }

        public Event GetEvent(int version)
        {
            return new SyndicationPublished { Version = version, Syndication = SyndicationUrl, Network = Network };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry
                && (
                    ((Entry)aggregate).Syndications == null
                    || ((Entry)aggregate).Syndications.Any(s=>s.Url == SyndicationUrl)
                );
        }
    }
}
