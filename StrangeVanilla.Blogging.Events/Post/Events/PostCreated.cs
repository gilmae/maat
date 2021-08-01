using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Events;
using mf;

namespace StrangeVanilla.Blogging.Events.Posts.Events
{
    public class PostCreated : Event<Post>
    {
        public string[] Type { get; set; }
        [JsonConverter(typeof(MicroformatPropertiesSerialiser))]
        public Dictionary<string, object[]> Properties { get; set; }
        public Microformat[] Children { get; set; }
        public int OwnerId { get; set; }

        public PostCreated() { }
        public PostCreated(Guid id)
        {
            AggregateId = id;
        }

        public override void Apply(Post aggregate)
        {
            base.Apply(aggregate);
            aggregate.CreatedAt = this.OccuredAt;
            aggregate.LastModifiedAt = this.OccuredAt;
            aggregate.Data = new Microformat { Children = new List<Microformat>(), Properties = Properties, Type = Type };
            if (Children != null)
            {
                aggregate.Data.Children.AddRange(Children);
            }
            aggregate.OwnerId = OwnerId;
        }
    }
}
