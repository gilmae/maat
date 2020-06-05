using System.Collections.Generic;
using System.Threading.Tasks;
using Events;

namespace SV.Maat.lib.Pipelines
{
    public class PipelineService
    {
        private readonly IDictionary<object, Pipeline> pipelines = new Dictionary<object, Pipeline>();

        public Pipeline Create<T>()
        {
            var pipeline = new Pipeline();
            pipelines.Add(typeof(T), pipeline);
            return pipeline;
        }

        public void Run<T>(Aggregate aggregate)
        {
            var pipeline = pipelines[typeof(T)].Build();
            if (pipeline == null) { return; }

            Task.Run(() =>
            {
                pipeline.Invoke(aggregate);
            });
        }
    }
}
