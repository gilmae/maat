using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class SetOwner :ICommand
    {
        public int OwnerId { get; set; }

        public SetOwner()
        {
        }

        public Event GetEvent(int version)
        {
            return new OwnerSet() { OwnerId = OwnerId, Version = version };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry && ((Entry)aggregate).OwnerId == default;
        }
    }
}
