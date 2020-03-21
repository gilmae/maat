using System;
using System.Collections.Generic;
using System.Linq;
using Events;

using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace StrangeVanilla.Maat.Commands
{
    public class UpdateEntryAsAddCommand : BaseCommand<Entry>
    {
        IEventStore<Entry> _entryStore;
        public Entry Entry { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string[] Categories { get; set; }
        public IEnumerable<Entry.MediaLink> Media { get; set; }
        public string BookmarkOf { get; set; }
        public bool Published { get; set; }
        public string ReplyTo { get; set; }

        public UpdateEntryAsAddCommand(IEventStore<Entry> entryStore)
        {
            _entryStore = entryStore;
        }

        public override  Entry Execute()
        {
            var events = new List<Event<Entry>>();

            Incrementor version = new Incrementor(_entryStore.GetCurrentVersion(Entry.Id));

            events.Add(new EntryUpdated(Entry.Id)
            {
                Body = Content,
                Title = Name,
                Version = version.Next(),
                BookmarkOf = BookmarkOf,
                ReplyTo = ReplyTo
            });

            if (Published)
            {
                events.Add(new EntryPublished(Entry.Id) { Version = version.Next() });
            }

            if (Categories != null)
            {
                events.AddRange(Categories.Select(c => new EntryCategorised(Entry.Id, c) { Version = version.Next() }));
            }

            if (Media != null)
            {
                events.AddRange(Media.Select(m => new MediaAssociated(Entry.Id, m.Url, m.Type, m.Description) { Version = version.Next() }));
            }

            _entryStore.StoreEvent(events);

            foreach (var e in events)
            {
                e.Apply(Entry);
            }

            return Entry;
        }
    }
}
