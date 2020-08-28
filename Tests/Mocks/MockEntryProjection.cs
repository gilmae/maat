using System;
using System.Collections.Generic;
using System.Linq;
using StrangeVanilla.Blogging.Events;
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

        public Entry Get(Guid id, bool isPublished = false)
        {
            return _entries.FirstOrDefault(e => e.Id == id);
        }

        public Entry Get(string slug)
        {
            return _entries.FirstOrDefault(e => e.Slug == slug);
        }

        public IEnumerable<Entry> GetAfter(int numItems, DateTime after, bool isPublished = false)
        {
            return _entries.Where(e => e.CreatedAt > after).OrderBy(e => e.CreatedAt).Take(numItems);
        }

        public IEnumerable<Entry> GetBefore(int numItems, DateTime before, bool isPublished = false)
        {
            return _entries.Where(e => e.CreatedAt < before).OrderBy(e => e.CreatedAt).Take(numItems);
        }

        public IEnumerable<Entry> GetLatest(int numItems, bool isPublished = false)
        {
            return _entries.OrderBy(e => e.CreatedAt).Take(numItems);
        }

    }
}
