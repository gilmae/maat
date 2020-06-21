using System;
using System.Linq;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class ClearMediaFromEntry : ICommand
    {
        public ClearMediaFromEntry()
        {
        }

        public Event GetEvent(int version)
        {
            return new MediaCleared();
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry;
        }
    }
}
