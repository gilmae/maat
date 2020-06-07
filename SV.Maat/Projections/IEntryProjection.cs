using System;
using System.Collections.Generic;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.Projections
{
    public interface IEntryProjection
    {
        Entry Get(Guid id);
        IEnumerable<Entry> GetAfter(int numItems, DateTime after);
        IEnumerable<Entry> GetBefore(int numItems, DateTime before);
        IEnumerable<Entry> GetLatest(int numItems);
    }
}
