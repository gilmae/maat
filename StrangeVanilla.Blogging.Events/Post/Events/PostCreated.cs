using System;
using System.Collections.Generic;
using Events;
using mf;

namespace StrangeVanilla.Blogging.Events.Posts.Events
{
    public class PostCreated : Event<Post>
    {
        public string[] Type { get; set; }
        public Dictionary<string, object[]> Properties { get; set; }
        public Microformat[] Children { get; set; }

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
        }
    }
}
