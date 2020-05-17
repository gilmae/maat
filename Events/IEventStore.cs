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
}
