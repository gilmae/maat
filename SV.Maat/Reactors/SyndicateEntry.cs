using System;
using System.Threading.Tasks;
using Events;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib.Pipelines;

namespace SV.Maat.Reactors
{
    public class SyndicateEntry
    {
        private readonly ILogger<SyndicateEntry> _logger;
        private readonly EventDelegate _next;
        public SyndicateEntry(ILogger<SyndicateEntry> logger, EventDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(Event e)
        {
            if (e is Syndicated)
            {
                var syndicatedEvent = e as Syndicated;
                _logger.LogTrace($"Syndicating {e.AggregateId} to {syndicatedEvent.SyndicationAccount}");
            }

            await _next(e);
        }
    }

    public static class SyndicateEntryUtils
    {
        public static PipelineBuilder UseSyndicateEntry(this PipelineBuilder pipeline)
        {
            return pipeline.UseReactor<SyndicateEntry>();
        }
    }
}
