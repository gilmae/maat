using System;
using System.Collections.Generic;
using System.Linq;
using Events;

using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace StrangeVanilla.Maat.Commands
{
    public class UpdateEntryAsRemoveCommand
    {
        IEventStore<Entry> _entryStore;
        

        public UpdateEntryAsRemoveCommand(IEventStore<Entry> entryStore)
        {
            _entryStore = entryStore;
        }

        public Entry Execute(Entry entry, bool name, bool content, bool categories, bool media, bool bookmarkOf, bool published)
        {
            var events = new List<Event<Entry>>();

            Incrementor version = new Incrementor(_entryStore.GetCurrentVersion(entry.Id));

            events.Add(new EntryUpdated(entry.Id)
            {
                Body = content ? "" : null,
                Title = name ? "" : null,
                Version = version.Next(),
                BookmarkOf = bookmarkOf ? "" : null
            });

            if (categories)
            {
                events.Add(new EntryCategoriesCleared(entry.Id) { Version = version.Next() });
            }

            if (media)
            {
                events.Add(new MediaCleared(entry.Id) { Version = version.Next() });
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
