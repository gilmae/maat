using System;
using Events;
using SV.Maat.Commands;
using SV.Maat.lib.Pipelines;

namespace SV.Maat
{
    public class CommandHandler
    {
        IEventStore _eventStore;
        Pipeline _pipeline;

        public CommandHandler(IEventStore eventStore, Pipeline pipeline)
        {
            _eventStore = eventStore;
            _pipeline = pipeline;
        }

        public bool Handle<T>(Aggregate aggregate, ICommand command) where T : Aggregate
        {
            var events = _eventStore.Retrieve(new EventScope() {AggregateType=aggregate.GetType(), AggregateId=aggregate.Id });
            aggregate.ReplayEvents(events);

            (Event newEvent, bool success) = aggregate.AttemptCommand(command);

            if (success)
            {
                newEvent.AggregateId = aggregate.Id;
                _eventStore.StoreEvent(newEvent);
                _pipeline.Run(newEvent);
            }

            return success;
        }
    }
}
