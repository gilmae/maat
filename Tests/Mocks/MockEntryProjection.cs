using System;
using System.Collections.Generic;
using System.Linq;
using SV.Maat.Micropub.Models;
using SV.Maat.Projections;

namespace Tests.Mocks
{
    public class MockEntryProjection : IEntryProjection
    {
        private IList<Entry> _entries;

        public MockEntryProjection(IList<Entry> entries)
        {
            _entries = entries;
        }

        public IEnumerable<Entry> Get()
        {
            return _entries;
        }

        public Entry Get(Guid id)
        {
            return _entries.FirstOrDefault(e => e.Id == id.ToString());
        }

        public Entry Get(string slug)
        {
            return _entries.FirstOrDefault(e => e.Url != null && e.Url.Any(s => !string.IsNullOrEmpty(s) && new Uri(s).AbsolutePath.ToString() == slug));
        }

        public IEnumerable<Entry> GetAfter(int numItems, DateTime after)
        {
            return _entries.Where(e => e.Created > after).OrderBy(e => e.Published).Take(numItems);
        }

        public IEnumerable<Entry> GetBefore(int numItems, DateTime before)
        {
            return _entries.Where(e => e.Created < before).OrderBy(e => e.Published).Take(numItems);
        }

        public IEnumerable<Entry> GetLatest(int numItems)
        {
            return _entries.OrderBy(e => e.Created).Take(numItems);
        }

    }
}
