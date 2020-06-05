using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Events;

namespace SV.Maat.lib.Pipelines
{
    public class Pipeline
    {
        private readonly IList<Func<AggregateDelegate, AggregateDelegate>> _components = new List<Func<AggregateDelegate, AggregateDelegate>>();

        public Pipeline Use(Func<AggregateDelegate, AggregateDelegate> component)
        {
            _components.Add(component);
            return this;
        }

        public Pipeline Use(Func<Aggregate, Func<Task>, Task> component)
        {
            return this.Use(next =>
            {
                return aggregate =>
                {
                    Func<Task> simpleNext = () => next(aggregate);
                    return component(aggregate, simpleNext);
                };
            });
        }

        public AggregateDelegate Build()
        {
            AggregateDelegate app = obj => Task.CompletedTask;

            foreach (var component in _components)
            {
                app = component(app);
            }
            return app;
        }
    }

    public class EventPipeline<T> where T:Aggregate
    {
        private readonly IList<Func<EventDelegate<T>, EventDelegate<T>>> _components = new List<Func<EventDelegate<T>, EventDelegate<T>>>();

        public EventPipeline<T> Use(Func<EventDelegate<T>, EventDelegate<T>> component)
        {
            _components.Add(component);
            return this;
        }

        public EventPipeline<T> Use(Func<Event<T>, Func<Task>, Task> component)
        {
            return this.Use(next =>
            {
                return e =>
                {
                    Func<Task> simpleNext = () => next(e);
                    return component(e, simpleNext);
                };
            });
        }

        public EventDelegate<T> Build()
        {
            EventDelegate<T> app = obj => Task.CompletedTask;

            foreach (var component in _components)
            {
                app = component(app);
            }
            return app;
        }

        public void Run(Event<T> e)
        {
            var app = this.Build();
           
            Task.Run(() =>
            {
                app.Invoke(e);
            });
        }
    }




}
