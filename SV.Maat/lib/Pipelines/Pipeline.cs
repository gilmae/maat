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

    
    

}
