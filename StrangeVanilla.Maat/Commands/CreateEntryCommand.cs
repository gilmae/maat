using System;
using System.Collections.Generic;
using System.Linq;
using Events;

using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace StrangeVanilla.Maat.Commands
{
    public class CreateEntryCommand
    {
        IEventStore<Entry> _entryStore;
        

        public CreateEntryCommand(IEventStore<Entry> entryStore)
        {
            _entryStore = entryStore;
        }

        public Entry Execute(string name, string content, string[] categories, string bookmarkOf, IEnumerable<Media> media, bool published)
        {
            var entry = new Entry();
            var events = new List<Event<Entry>>();

            Incrementor version = new Incrementor(_entryStore.GetCurrentVersion(entry.Id));

            events.Add(new EntryAdded(entry)
            {
                Body = content,
                Title = name,
                Version = version.Next(),
                BookmarkOf = bookmarkOf
            });

            if (published)
            {
                events.Add(new EntryPublished(entry));
            }

            if (categories != null)
            {
                events.AddRange(categories.Select(c => new EntryCategorised(entry, c) { Version = version.Next() }));
            }

            if (media != null)
            {
                events.AddRange(media.Select(m => new MediaAssociated(entry, m) { Version = version.Next() }));

            }

            _entryStore.StoreEvent(events);

            foreach (var e in events)
            {
                entry = e.Apply(entry);
            }

            return entry;
        }
    }
}
