using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Events;
using Microsoft.Extensions.Logging;

namespace SV.Maat
{
    public class MemoryProjection<T> : IProjection<T> where T : Aggregate
    {
        ConcurrentDictionary<Guid, T> projections = new ConcurrentDictionary<Guid, T>();
        ILogger<MemoryProjection<T>> _logger;
        Mutex m = new Mutex();
        int lastIdProcessed = -1;

        public MemoryProjection(ILogger<MemoryProjection<T>> logger, IEventStore<T> _aggregateRepository)
        {
            _logger = logger;

            var events = _aggregateRepository.Retrieve();
            var groupedEvents = events.GroupBy(e => e.AggregateId);

            foreach (IGrouping<Guid, Event<T>> a in groupedEvents)
            {
                T aggregate = (T)Activator.CreateInstance(typeof(T), a.Key);
                foreach (Event<T> e in a)
                {
                    e.Apply(aggregate);
                }

                projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);
            }
            lastIdProcessed = events.Max(e => e.Id);
            _logger.LogTrace("{0} memory projection created, last id processed: {1}", typeof(T).Name, lastIdProcessed);

            Task.Run(() =>
            {
                while (true)
                {
                    int sleepTime = 5;
                    try
                    {
                        var records = _aggregateRepository.RetrieveAfter(lastIdProcessed);

                        if (records.Count() == 0)
                        {
                            sleepTime = 5;
                        }
                        else
                        {
                            _logger.LogTrace("Processing {0} new {1} events", records.Count(), typeof(T).Name);
                            foreach (Event<T> e in records)
                            {
                                if (!projections.TryGetValue(e.AggregateId, out T aggregate))
                                {
                                    aggregate = (T)Activator.CreateInstance(typeof(T), e.AggregateId);
                                }
                                e.Apply(aggregate);
                                projections.AddOrUpdate(aggregate.Id, aggregate, (key, oldValue) => aggregate);
                                lastIdProcessed = records.Max(e => e.Id);
                            }
                            _logger.LogTrace("{0} memory projection updated, last id processed: {1}", typeof(T).Name, lastIdProcessed);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        _logger.LogError(ex.StackTrace);
                    }
                    finally
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
                    }
                }
            });
        }

        public T Get(Guid id)
        {
            return projections[id];
        }

        public IEnumerable<T> Get()
        {
            return projections.Values;
        }


    }
}
