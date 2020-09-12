using System;
using System.Collections.Generic;
using StrangeVanilla.Blogging.Events;
using System.Linq;

namespace SV.Maat.Projections
{
    public interface IEntryProjection
    {
        Entry Get(Guid id);
        Entry Get(string slug);
        IEnumerable<Entry> Get();
        IEnumerable<Entry> GetAfter(int numItems, DateTime after);
        IEnumerable<Entry> GetBefore(int numItems, DateTime before);
        IEnumerable<Entry> GetLatest(int numItems);
    }
}
