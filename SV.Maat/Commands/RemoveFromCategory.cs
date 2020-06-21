using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class RemoveFromCategory : ICommand
    {
        public RemoveFromCategory()
        {
        }

        public string Category { get; set; }

        public Event GetEvent(int version)
        {
            return new EntryCategoryRemoved { Category = Category };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry && ((Entry)aggregate).Categories.Contains(Category);
        }
    }
}
