using System;
using System.Collections.Generic;
using System.Linq;
using Events;

using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace StrangeVanilla.Maat.Commands
{
    public class UpdateEntryAsReplaceCommand
    {
        IEventStore<Entry> _entryStore;
        

        public UpdateEntryAsReplaceCommand(IEventStore<Entry> entryStore)
        {
            _entryStore = entryStore;
        }

        public Entry Execute(Entry entry, string name, string content, string[] categories, IEnumerable<Entry.MediaLink> media, string bookmarkOf, bool published)
        {
            var events = new List<Event<Entry>>();

            Incrementor version = new Incrementor(_entryStore.GetCurrentVersion(entry.Id));

            events.Add(new EntryUpdated(entry.Id)
            {
                Body = content,
                Title = name,
                Version = version.Next(),
                BookmarkOf = bookmarkOf
            });

            if (published)
            {
                events.Add(new EntryPublished(entry.Id));
            }

            if (categories != null)
            {
                events.Add(new EntryCategoriesCleared(entry.Id) { Version = version.Next() });
                events.AddRange(categories.Select(c => new EntryCategorised(entry.Id, c) { Version = version.Next() }));
            }

            if (media != null)
            {
                events.Add(new MediaCleared(entry.Id) { Version = version.Next() });
                events.AddRange(media.Select(m => new MediaAssociated(entry.Id, m.Url, m.Type, m.Description) { Version = version.Next() }));

            }

            _entryStore.StoreEvent(events);

            foreach (var e in events)
            {
                e.Apply(entry);
            }

            return entry;
        }
    }
}
