using System.Threading.Tasks;
using Events;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.lib.Pipelines
{
    public class PopulateEntryProjection
    {
        IProjection<Entry> _projection;
        AggregateDelegate _next;
        public PopulateEntryProjection(AggregateDelegate next, IProjection<Entry> projection)
        {
            _next = next;
            _projection = projection;
        }

        public async Task InvokeAsync(Aggregate aggregate)
        {
            if (aggregate is Entry)
            {
                _projection.Add(aggregate as Entry);
            }
            await _next(aggregate);
        }
    }
}
