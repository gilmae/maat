using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class Syndicate : ICommand
    {
        public Syndicate()
        {
        }

        public string SyndicationAccount { get; set; }

        public Event GetEvent(int version)
        {
            return new Syndicated { SyndicationAccount = SyndicationAccount };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry
                && (
                    ((Entry)aggregate).SyndicateTo == null
                    || ((Entry)aggregate).SyndicateTo.Contains(SyndicationAccount)
                );
        }
    }
}
