using System;
using Events;
using mf;

namespace StrangeVanilla.Blogging.Events
{
    public record Post : Aggregate
    {
        public Microformat Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }

        public Post() : base() { }
        public Post(Guid id) : base(id) { }
    }
}
