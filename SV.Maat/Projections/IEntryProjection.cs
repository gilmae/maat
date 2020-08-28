using System;
using System.Collections.Generic;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.Projections
{
    public interface IEntryProjection
    {
        Entry Get(Guid id, bool publishedOnly = false);
        Entry Get(string slug);
        IEnumerable<Entry> GetAfter(int numItems, DateTime after, bool publishedOnly = false);
        IEnumerable<Entry> GetBefore(int numItems, DateTime before, bool publishedOnly = false);
        IEnumerable<Entry> GetLatest(int numItems, bool publishedOnly = false);
    }
}
