
using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class ReplyTo : ICommand
    {
        public string ReplyToUrl { get; set; }

        public ReplyTo()
        {
        }

        public Event GetEvent(int version)
        {
            return new ReplyingTo() { Version = version, ReplyTo = ReplyToUrl };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry;
        }
    }
}

