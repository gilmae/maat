using System;
using System.Linq;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class AttachMediaToEntry : ICommand
    {
        public AttachMediaToEntry()
        {
        }

        public string Url { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public Event GetEvent(int version)
        {
            return new MediaAssociated() { Description = Description, Type = Type, Url = Url };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry
                && (((Entry)aggregate).AssociatedMedia == null
                || ((Entry)aggregate).AssociatedMedia.Any(m=>m.Url == Url));
        }
    }
}
