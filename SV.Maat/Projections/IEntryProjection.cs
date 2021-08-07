using System;
using System.Collections.Generic;
using StrangeVanilla.Blogging.Events;
using mfv = SV.Maat.Micropub.Models;

namespace SV.Maat.Projections
{
    public interface IEntryProjection
    {
        mfv.Entry Get(Guid id);
        mfv.Entry Get(string slug);
        IEnumerable<mfv.Entry> Get();
        IEnumerable<mfv.Entry> GetAfter(int numItems, DateTime after);
        IEnumerable<mfv.Entry> GetBefore(int numItems, DateTime before);
        IEnumerable<mfv.Entry> GetLatest(int numItems);
    }
}
