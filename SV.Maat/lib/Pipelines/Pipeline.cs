using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Events;

namespace SV.Maat.lib.Pipelines
{
    public class Pipeline 
    {
        private readonly IList<Func<EventDelegate, EventDelegate>> _components = new List<Func<EventDelegate, EventDelegate>>();

        public Pipeline Use(Func<EventDelegate, EventDelegate> component)
        {
            _components.Add(component);
            return this;
        }

        public Pipeline Use(Func<Event, Func<Task>, Task> component)
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

        public EventDelegate Build()
        {
            EventDelegate app = obj => Task.CompletedTask;

            foreach (var component in _components)
            {
                app = component(app);
            }
            return app;
        }

        public void Run(Event e)
        {
            var app = this.Build();
           
            Task.Run(() =>
            {
                app.Invoke(e);
            });
        }
    }
}