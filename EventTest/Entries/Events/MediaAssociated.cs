using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class MediaAssociated : Event<Entry>
    {
        public MediaAssociated() { }
        public MediaAssociated(Guid entryId, Media media)
        {
            AggregateId = entryId;
            MediaId = media.MediaStoreId;
        }

        public string MediaId { get; set; }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            if (aggregate.AssociatedMedia == null)
            {
                aggregate.AssociatedMedia = new List<string>();
            }
            aggregate.AssociatedMedia.Add(MediaId);
            
        }
    }
}
