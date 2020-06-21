using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace SV.Maat.Commands
{
    public class PublishEntry : ICommand
    {
        public PublishEntry()
        {
        }

        public Event GetEvent(int version)
        {
            return new EntryPublished() { Version = version, PublishedAt = DateTime.UtcNow };
        }

        public bool IsValid(Aggregate aggregate)
        {
            return aggregate is Entry && !(aggregate as Entry).PublishedAt.HasValue;
        }
    }
}
