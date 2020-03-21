using System;
using System.Collections.Generic;
using System.Linq;
using Events;

using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace StrangeVanilla.Maat.Commands
{
    public class UpdateEntryAsRemoveCommand : BaseCommand<Entry>
    {
        IEventStore<Entry> _entryStore;

        public bool Name { get; set; }
        public bool Content { get; set; }
        public bool Categories { get; set; }
        public bool Media { get; set; }
        public bool BookmarkOf { get; set; }
        public Entry Entry { get; set; }
        public bool ReplyTo { get; set; }

        public UpdateEntryAsRemoveCommand(IEventStore<Entry> entryStore)
        {
            _entryStore = entryStore;
        }

        public override Entry Execute()
        {
            var events = new List<Event<Entry>>();

            Incrementor version = new Incrementor(_entryStore.GetCurrentVersion(Entry.Id));

            events.Add(new EntryUpdated(Entry.Id)
            {
                Body = Content ? "" : null,
                Title = Name ? "" : null,
                Version = version.Next(),
                BookmarkOf = BookmarkOf ? "" : null,
                ReplyTo = ReplyTo ? "" : null
            });

            if (Categories)
            {
                events.Add(new EntryCategoriesCleared(Entry.Id) { Version = version.Next() });
            }

            if (Media)
            {
                events.Add(new MediaCleared(Entry.Id) { Version = version.Next() });
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
