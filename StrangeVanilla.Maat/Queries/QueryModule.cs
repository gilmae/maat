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
    public class QueryModule : NancyModule
    {
        IProjection<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;

        public QueryModule(ILogger<NancyModule> logger, IProjection<Entry> entryRepository, IEventStore<Media> mediaRepository)
        {
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;

            Get("/q/entry/{id:Guid}",

                p =>
                {
                    
                    return JsonConvert.SerializeObject(entryRepository.Get(p.id));
                }
                );
        }
    }
}
