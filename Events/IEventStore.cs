using System;
using System.Collections.Generic;

namespace Events
{
    public interface IEventStore<T> where T : Aggregate
    {
        long StoreEvent(Event<T> e);
        long StoreEvent(IList<Event<T>> e);

        IList<Event<T>> Retrieve(Guid id);
        IList<Event<T>> Retrieve();
        IList<Event<T>> RetrieveAfter(int id);

        int? GetCurrentVersion(Guid id);

    }

    public interface IEventStore
    {
        long StoreEvent(Event e);

        IList<Event> Retrieve();
        IList<Event> Retrieve(EventScope scope);
    }

    public struct EventScope
    {
        public int? After { get; set; }
        public Type AggregateType { get; set; }
        public Guid? AggregateId { get; set; }
    }
}
