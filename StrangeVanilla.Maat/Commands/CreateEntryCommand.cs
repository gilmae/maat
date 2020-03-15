using System;
using System.Collections.Generic;
using System.Linq;
using Events;

using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;

namespace StrangeVanilla.Maat.Commands
{
    public class CreateEntryCommand : BaseCommand<Entry>
    {
        IEventStore<Entry> _entryStore;

        public string Name { get; set; }
        public string Content { get; set; }
        public string[] Categories { get; set; }
        public IEnumerable<Entry.MediaLink> Media { get; set; }
        public string BookmarkOf { get; set; }
        public bool Published { get; set; }

        public CreateEntryCommand(IEventStore<Entry> entryStore)
        {
            _entryStore = entryStore;
        }

        public override Entry Execute()
        {
            var entry = new Entry();
            var events = new List<Event<Entry>>();

            Incrementor version = new Incrementor(_entryStore.GetCurrentVersion(entry.Id));

            events.Add(new EntryAdded(entry.Id)
            {
                Body = Content,
                Title = Name,
                Version = version.Next(),
                BookmarkOf = BookmarkOf
            });

            if (Published)
            {
                events.Add(new EntryPublished(entry.Id) { Version = version.Next() });
            }

            if (Categories != null)
            {
                events.AddRange(Categories.Select(c => new EntryCategorised(entry.Id, c) { Version = version.Next() }));
            }

            if (Media != null)
            {
                events.AddRange(Media.Select(m => new MediaAssociated(entry.Id, m.Url, m.Type, m.Description) { Version = version.Next() }));
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
