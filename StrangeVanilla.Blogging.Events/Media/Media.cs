using System;
using Events;

namespace StrangeVanilla.Blogging.Events
{
    public record Media : Aggregate
    {
        public string Name { get; set; }
        public string MediaStoreId { get; set; }
        public string MimeType { get; set; }

        public Media() : base() { }
        public Media(Guid id) : base(id) { }
    }
}
