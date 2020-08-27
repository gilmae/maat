using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class SetSlug :ICommand
    {
        public string Slug { get; set; }

        public SetSlug()
        {
        }

        public Event GetEvent(int version)
        {
            return new SlugSet() { Slug = Slug, Version = version };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry;
        }
    }
}
