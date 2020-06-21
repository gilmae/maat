using System;
using Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class CreateEntry : ICommand
    {
        public CreateEntry()
        {
        }

        public Event GetEvent(int version)
        {
            return new EntryCreated() { Version = version };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate.Version == 0;
        }
    }
}
