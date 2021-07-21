using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events.Posts.Events
{
    public class PropertiesRemovedFromPost : Event<Post>
    {
        public string[] Properties { get; set; }
        

        public PropertiesRemovedFromPost() { }
        public PropertiesRemovedFromPost(Guid id)
        {
            AggregateId = id;
        }

        public override void Apply(Post aggregate)
        {
            base.Apply(aggregate);
            foreach (string key in Properties)
            {
                aggregate.Data.Properties.Remove(key);
            }
            aggregate.LastModifiedAt = this.OccuredAt;
        }
    }
}
