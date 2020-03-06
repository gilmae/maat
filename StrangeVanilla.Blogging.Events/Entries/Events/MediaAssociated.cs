using System;
using System.Collections.Generic;
using System.Linq;
using Events;

namespace StrangeVanilla.Blogging.Events.Entries.Events
{
    public class MediaAssociated : Event<Entry>
    {
        public MediaAssociated() { }
        public MediaAssociated(Guid entryId, string url, string type, string description)
        {
            AggregateId = entryId;
            Url = url;
            Type = type;
            Description = description;
        }

        public string Url { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public override void Apply(Entry aggregate)
        {
            base.Apply(aggregate);
            if (aggregate.AssociatedMedia == null)
            {
                aggregate.AssociatedMedia = new List<Entry.MediaLink>();
            }
            aggregate.AssociatedMedia.Add(new Entry.MediaLink { Url = Url, Type = Type, Description=Description });
            
        }
    }
}
