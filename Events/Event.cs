using System;

namespace Events
{
    public abstract class Event<T> where T : Aggregate
    {
        public int Id { get; set; }
        public long SequencePoint { get; set; }
        public DateTime OccuredAt { get; set; }
        public Guid AggregateId { get; set; }

        public Event()
        {
            OccuredAt = DateTime.UtcNow;
            SequencePoint = DateTime.UtcNow.Ticks;
        }

        public abstract T Apply(T aggregate);
    }
}
