//using System;
//using Events;

//namespace StrangeVanilla.Blogging.Events.Entries.Events
//{
//    public class EntryAdded : Event<Entry>
//    {
//        public string Title { get; set; }
//        public string Body { get; set; }
//        public string BookmarkOf { get; set; }
//        public DateTime CreatedAt { get; set; }

//        public EntryAdded() { CreatedAt = DateTime.UtcNow; }
//        public EntryAdded(Guid entryId)
//        {
//            AggregateId = entryId;
//        }

//        public override void Apply(Entry aggregate)
//        {
//            base.Apply(aggregate);
//            aggregate.Body = Body;
//            aggregate.Title = Title;
//            aggregate.BookmarkOf = BookmarkOf;
//            aggregate.CreatedAt = CreatedAt;
//        }
//    }
//}