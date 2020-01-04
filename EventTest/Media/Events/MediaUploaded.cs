﻿using System;
using Events;

namespace StrangeVanilla.Blogging.Events
{
    public class MediaUploaded : Event<Media>
    {
        public MediaUploaded()
        {
            AggregateId = Guid.NewGuid();
        }

        public string Name { get; set; }
        public string MimeType { get; set; }
        public string MediaStoreId { get; set; }

        public override Media Apply(Media aggregate)
        {
            aggregate.Id = AggregateId;
            aggregate.Name = Name;
            aggregate.MediaStoreId = MediaStoreId;
            aggregate.MimeType = MimeType;
            return aggregate;
        }
    }
}
