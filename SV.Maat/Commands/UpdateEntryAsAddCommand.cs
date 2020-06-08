using System;
using System.Collections.Generic;
using System.Linq;
using Events;

using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Blogging.Events.Entries.Events;
using SV.Maat.lib;
using SV.Maat.lib.Pipelines;

namespace SV.Maat.Commands
{
    public class UpdateEntryAsAddCommand : BaseCommand<Entry>
    {
        IEventStore<Entry> _entryStore;
        Pipeline _pipeline;

        public Entry Entry { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string[] Categories { get; set; }
        public IEnumerable<Entry.MediaLink> Media { get; set; }
        public string BookmarkOf { get; set; }
        public bool Published { get; set; }
        public string ReplyTo { get; set; }
        public string[] SyndicateTo { get; set; }

        public UpdateEntryAsAddCommand(IEventStore<Entry> entryStore, Pipeline pipeline)
        {
            _entryStore = entryStore;
            _pipeline = pipeline;
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
                BookmarkOf = BookmarkOf
            });

            if (!string.IsNullOrEmpty(ReplyTo))
            {
                events.Add(new ReplyingTo(ReplyTo, Entry.Id));
            }

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

            if (SyndicateTo != null)
            {
                events.AddRange(SyndicateTo.Select(s => new Syndicated(Entry.Id, s)));
            }

            _entryStore.StoreEvent(events);

            foreach (var e in events)
            {
                e.Apply(Entry);
                _pipeline.Run(e);
            }

            return Entry;
        }
    }
}
