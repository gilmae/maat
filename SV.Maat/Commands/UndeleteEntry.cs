using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib;

namespace SV.Maat.Commands
{
    public class UndeleteEntry : ICommand
    {
        public Event GetEvent(int version)
        {
            return new EntryUndeleted();
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry && ((Entry)aggregate).DeletedAt != null;
        }
    }
}
