using System;
using System.Collections.Generic;
using Events;

namespace StrangeVanilla.Blogging.Events
{
    public class Entry : Aggregate
    {
        public Content Title { get; set; }
        public Content Body { get; set; }
        public IList<string> Categories { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string Uid { get; set; }
        public IList<Syndication> Syndications { get; set; }
        public IList<MediaLink> AssociatedMedia { get; set; }
        public string BookmarkOf { get; set; }
        public string ReplyTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<string> SyndicateTo { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int OwnerId { get; set; }

        public string Slug { get; set; }

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

        public class Syndication
        {
            public string Url { get; set; }
            public string Network { get; set; }
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

    public enum MediaType
    {
        photo,
        video,
        document,
        archivedCopy
    }

    public class Content
    {
        public ContentType Type { get; set; }
        public string Value { get; set; }
        public string Markup { get;set; }
    }
}
