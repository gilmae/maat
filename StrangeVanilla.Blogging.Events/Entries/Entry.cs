using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events
{
    public class Entry : Aggregate
    {
        public string[] Title { get; set; }
        public KeyValuePair<ContentType, string>[] Body { get; set; }
        public IList<string> Categories { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string Uid { get; set; }
        public IList<string> Syndications { get; set; }
        public IList<MediaLink> AssociatedMedia { get; set; }
        public string BookmarkOf { get; set; }
        public string ReplyTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<string> SyndicateTo { get; set; }

        public DateTime? DeletedAt { get; set; }

        public StatusType Status
        {
            get
            {
                if (DeletedAt.HasValue)
                {
                    return StatusType.deleted;
                }
                else if (PublishedAt.HasValue)
                {
                    return StatusType.published;
                }
                return StatusType.published;
            }
        }

        public Entry() : base() { }
        public Entry(Guid id) : base(id) { }

        public class MediaLink
        {
            public string Url { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }
    }

    public enum StatusType{
        draft,
        published,
        deleted
    }

    public enum ContentType
    {
        plaintext,
        html,
        markdown
    }
}
