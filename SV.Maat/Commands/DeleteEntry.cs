using System;
using Events;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib;

namespace SV.Maat.Commands
{
    public class DeleteEntry : BaseCommand<Entry>
    {
        IEventStore<Entry> _entryStore;
        public Entry Entry { get; set; }

        public DeleteEntry(IEventStore<Entry> entryStore)
        {
            _entryStore = entryStore;
        }

        public override Entry Execute()
        {
            Incrementor version = new Incrementor(_entryStore.GetCurrentVersion(Entry.Id));

            var e = new EntryDeleted(Entry.Id) { Version = version.Next() };

            _entryStore.StoreEvent(e);

            e.Apply(Entry);

            return Entry;
        }
    }
}
