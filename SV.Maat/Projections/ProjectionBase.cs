using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Events;
using Microsoft.Extensions.Logging;

namespace SV.Maat.Projections
{
    public abstract class ProjectionBase
    {
        protected int lastIdProcessed = -1;
        protected readonly IEventStore eventStore;
        protected readonly  ILogger logger;

        public ProjectionBase(ILogger logger, IEventStore eventStore)
        {
            this.eventStore = eventStore;
            this.logger = logger;
            Task.Run(() =>
            {
                while (true)
                {
                    int sleepTime = 5;
                    int? lastIdInEvents = null;
                    try
                    {
                        var events = this.eventStore.Retrieve(new EventScope { After = lastIdProcessed });

                        (lastIdInEvents, sleepTime) = ProcessEvents(events);
                        if (lastIdInEvents.HasValue)
                        {
                            lastIdProcessed = lastIdInEvents.Value;
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
                    }
                }
            });
        }

        public abstract (int?,int) ProcessEvents(IList<Event> newEvents);
    }
}
