using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events
{
    public class Entry : Aggregate
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public IList<string> Categories { get; set; }
        public DateTime Published_At { get; set; }
        public string Uid { get; set; }
        public IList<string> Syndications { get; set; }
        public IList<string> AssociatedMedia { get; set; }
        public string BookmarkOf { get; set; }
        
        public Entry() : base() { }
        public Entry(Guid id) : base(id) { }
    }
}
