using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class MediaAssociated : Event<Entry>
    {
        public MediaAssociated() { }
        public MediaAssociated(Entry entry, Media media)
        {
            AggregateId = entry.Id;
            MediaId = media.Id;
        }

        public Guid MediaId { get; set; }

        public override Entry Apply(Entry aggregate)
        {
            if (aggregate.AssociatedMedia == null)
            {
                aggregate.AssociatedMedia = new List<Guid>();
            }
            if (!aggregate.AssociatedMedia.Any(id => id == MediaId))
            {
                aggregate.AssociatedMedia.Add(MediaId);
            }

            return aggregate;
        }
    }
}
