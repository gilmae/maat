using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class AddToCategory : ICommand
    {
        public string Category { get; set; }

        public AddToCategory()
        {
        }

        public Event GetEvent(int version)
        {
            return new EntryCategorised() { Version = version, Category = Category };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry && (((Entry)aggregate).Categories == null || ((Entry)aggregate).Categories.Contains(Category));
        }
    }
}