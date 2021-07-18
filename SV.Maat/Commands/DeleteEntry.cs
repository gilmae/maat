using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib;

namespace SV.Maat.Commands
{
    public class DeleteEntry : ICommand
    {
        public Event GetEvent(int version)
        {
            return new EntryDeleted() { Version = version };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry && ((Entry)aggregate).DeletedAt == null;
        }
    }
}
