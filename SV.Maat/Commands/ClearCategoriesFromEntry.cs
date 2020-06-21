using System;
using System.Linq;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class ClearCategoriesFromEntry : ICommand
    {
        public ClearCategoriesFromEntry()
        {
        }

        public Event GetEvent(int version)
        {
            return new EntryCategoriesCleared();
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry;
        }
    }
}
