using System;
using System.Collections.Generic;
using System.Linq;

namespace Events
{
    public class MemoryStore<T> : IEventStore<T> where T : Aggregate
    {
        public IList<Event<T>> _events;

        public MemoryStore()
        {
            _events = new List<Event<T>>();
        }

        public int? GetCurrentVersion(Guid id) => _events.Where(e=>e.AggregateId == id)?.Max(e => e.Version);

        public IList<Event<T>> Retrieve(Guid id)
        {
            return _events.Where(e => e.AggregateId == id).OrderBy(e => e.Version).ToList();
        }

        public IList<Event<T>> Retrieve()
        {
            return _events.OrderBy(e=>e.AggregateId).OrderBy(e => e.Version).ToList();
        }

        public long StoreEvent(Event<T> e)
        {
            return StoreEvent(new[] { e });
        }

        public long StoreEvent(IList<Event<T>> events)
        {
            foreach (Event<T> e in events)
            {
                _events.Add(e);
            }
            return _events.Last().Version;
        }
    }
}
