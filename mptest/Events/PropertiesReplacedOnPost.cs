using System;
using System.Collections.Generic;
using Events;

namespace mptest.Events
{
    public class PropertiesAddedToPost : Event<Post>
    {
        public Dictionary<string, object[]> Properties { get; set; }
        

        public PropertiesAddedToPost() { }
        public PropertiesAddedToPost(Guid entryId)
        {
            AggregateId = entryId;
        }

        public override void Apply(Post aggregate)
        {
            base.Apply(aggregate);
            foreach ((string key, object[] value) in Properties)
            {
                if (!aggregate.Data.Properties.ContainsKey(key))
                {
                    aggregate.Data.Properties[key] = value;
                }
                else
                {
                    List<object> newProperties = new List<object>(aggregate.Data.Properties[key]);
                    newProperties.AddRange(value);
                    aggregate.Data.Properties[key] = newProperties.ToArray();
                }
            }
            aggregate.LastModifiedAt = this.OccuredAt;
        }
    }
}
