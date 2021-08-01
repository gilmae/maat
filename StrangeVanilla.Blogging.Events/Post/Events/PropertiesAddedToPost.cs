using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Events;

namespace StrangeVanilla.Blogging.Events.Posts.Events
{
    public class PropertiesAddedToPost : Event<Post>
    {
        [JsonConverter(typeof(MicroformatPropertiesSerialiser))]
        public Dictionary<string, object[]> Properties { get; set; }
        

        public PropertiesAddedToPost() { }
        public PropertiesAddedToPost(Guid id)
        {
            AggregateId = id;
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
