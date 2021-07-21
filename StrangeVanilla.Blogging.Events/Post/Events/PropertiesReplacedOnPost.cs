﻿using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events.Posts.Events
{
    public class PropertiesReplacedOnPost : Event<Post>
    {
        public Dictionary<string, object[]> Properties { get; set; }
        

        public PropertiesReplacedOnPost() { }
        public PropertiesReplacedOnPost(Guid id)
        {
            AggregateId = id;
        }

        public override void Apply(Post aggregate)
        {
            base.Apply(aggregate);
            foreach ((string key, object[] value) in Properties)
            {

                aggregate.Data.Properties[key] = value;

            }
            aggregate.LastModifiedAt = this.OccuredAt;
        }
    }
}