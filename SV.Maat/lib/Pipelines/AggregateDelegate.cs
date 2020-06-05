using System.Threading.Tasks;
using Events;

namespace SV.Maat.lib.Pipelines
{
    public delegate Task AggregateDelegate(Aggregate entry);

    public delegate Task EventDelegate<T>(Event<T> entry) where T:Aggregate;
}
