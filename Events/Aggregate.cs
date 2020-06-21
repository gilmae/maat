using System;
using System.Collections.Generic;
using System.Linq;

namespace Events
{
    public abstract class Aggregate 
    {
        public Guid Id { get; set; }
        public int Version { get; set; }

        public Aggregate() : this(Guid.NewGuid())
        {
            
        }

        public Aggregate(Guid id)
        {
            Id = id;
        }

        public void ReplayEvents(IEnumerable<Event> events)
        {
            foreach (Event e in events.OrderBy(e=>e.Version))
            {
                e.Apply(this);
            }
        }

        /*
         * var events = EventStore.FindByAggregate(id);
         * var aggregate = new Aggregate(id);
         * aggregate.ReplayEvents(events);
         * (Event newEvent, bool succeeded) = aggregate.AttemptCommand(command);
         * if (succeeded) {
         *    EventStore.Put(newEvent);
         * }
         */
    }
}
