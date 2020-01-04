using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using Newtonsoft.Json;
using StrangeVanilla.Blogging.Events;

namespace StrangeVanilla.Maat.Indexing
{
    public class IndexingModule : Nancy.NancyModule
    {
        IEventStore<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;

        public IndexingModule(ILogger<NancyModule> logger, IEventStore<Entry> entryRepository, IEventStore<Media> mediaRepository)
        {
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;

            Get("/index", p => {
                var entries = new List<Entry>();

                var events = _entryRepository.Retrieve().GroupBy(e=>e.AggregateId);

                foreach(IGrouping<Guid, Event<Entry>> a in events)
                {
                    Entry entry = new Entry(a.Key);
                    foreach (Event<Entry> e in a)
                    {
                        entry = e.Apply(entry);
                    }

                    entries.Add(entry);
                }

                return JsonConvert.SerializeObject(entries);


            });
        }
    }
}
