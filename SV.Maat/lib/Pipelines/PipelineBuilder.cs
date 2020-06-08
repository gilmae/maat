using System;
using System.Linq;
using System.Reflection;
using Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SV.Maat.lib.Pipelines
{
    public class PipelineBuilder
    {
        internal const string InvokeMethodName = "Invoke";
        internal const string InvokeAsyncMethodName = "InvokeAsync";
        private readonly IServiceProvider _services;
        private readonly Pipeline _pipeline; 
        private readonly ILogger<PipelineBuilder> _logger;

        public PipelineBuilder(IServiceProvider services, Pipeline pipeline, ILogger<PipelineBuilder> logger)
        {
            _services = services;
            _pipeline = pipeline;
            _logger = logger;
        }

        public PipelineBuilder UseReactor(Action<Event> reactor)
        {
            _pipeline.Use(async (Event e, System.Func<System.Threading.Tasks.Task> next) =>
            {
                reactor(e);
                await next();
            });

            return this;
        }

        public PipelineBuilder UseReactor<T>()
        {
            return UseReactor(typeof(T));
        }

        public PipelineBuilder UseReactor(Type type)
        {
            _logger.LogTrace("Adding {0} reactor", type.Name);
            _pipeline.Use(next =>
            {
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                var invokeMethods = methods.Where(m =>
                    string.Equals(m.Name, InvokeMethodName, StringComparison.Ordinal)
                    || string.Equals(m.Name, InvokeAsyncMethodName, StringComparison.Ordinal)
                    ).ToArray();

                var reactor = ActivatorUtilities.CreateInstance(_services, type, next);
                return (EventDelegate) invokeMethods[0].CreateDelegate(typeof(EventDelegate), reactor);
            });

            return this;
        }
    }
}
