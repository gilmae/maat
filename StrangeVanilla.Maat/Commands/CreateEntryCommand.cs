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

        public Entry Execute(string name, string content, string[] categories, IEnumerable<Media> media)
        {
            var entry = new Entry();
            var events = new List<Event<Entry>>();

            events.Add(new EntryAdded(entry)
            {
                Body = content,
                Title = name
            });

            if (categories != null)
            {
                events.AddRange(categories.Select(c => new EntryCategorised(entry, c)));
            }

            if (media != null)
            {
                events.AddRange(media.Select(m => new MediaAssociated(entry, m)));

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
