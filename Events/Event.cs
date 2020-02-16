using System;

namespace Events
{
    public abstract class Event<T> where T : Aggregate
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid AggregateId { get; set; }

        public Event()
        {
            OccuredAt = DateTime.UtcNow;
        }

        public virtual void Apply(T aggregate)
        {
            aggregate.Version = Version;
        }
    }
}
