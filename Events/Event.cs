using System;

namespace Events
{
    public abstract class Event<T> : Event where T : Aggregate
    {
        public virtual void Apply(T aggregate)
        {
            aggregate.Version = Version;
        }

        public override Type AggregateType()
        {
            return typeof(T);
        }
    }

    public abstract class Event
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid AggregateId { get; set; }

        public Event()
        {
            OccuredAt = DateTime.UtcNow;
        }

        public virtual void Apply(Aggregate aggregate)
        {
            aggregate.Version = Version;
        }

        public virtual Type AggregateType()
        {
            return typeof(Aggregate);
        }
    }
}
