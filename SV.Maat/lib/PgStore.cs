using System;
using System.Collections.Generic;
using Events;
using Npgsql;
using Dapper;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SV.Maat.lib;

namespace SV.Maat
{
    public class PgStore<T> : PgStoreBase, IEventStore<T>  where T : Aggregate
    {
        readonly string _type = typeof(T).Name;

        public PgStore(IConfiguration configuration) : base(configuration)
        {
        }

        public IList<Event<T>> Retrieve(Guid id)
        {
            return base.Retrieve(new EventScope { AggregateType = typeof(T), AggregateId = id })
                .Select(x => (Event<T>)x).ToList();
        }

        public new IList<Event<T>> Retrieve()
        {
            return base.Retrieve(new EventScope { AggregateType = typeof(T) })
               .Select(x => (Event<T>)x).ToList();
        }

        public IList<Event<T>> RetrieveAfter(int id)
        {
            return base.Retrieve(new EventScope { AggregateType = typeof(T), After = id })
               .Select(x => (Event<T>)x).ToList();
        }

        public long StoreEvent(Event<T> e)
        {
            return StoreEvent(new[] { e });
        }

        public long StoreEvent(IList<Event<T>> events)
        {
            long lastVersion = 0;
            foreach(var e in events)
            {
                lastVersion = base.StoreEvent(e);
            }

            return lastVersion;
        }
    }
}
