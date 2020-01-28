using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using Newtonsoft.Json;
using StrangeVanilla.Blogging.Events;

namespace StrangeVanilla.Maat.Micropub
{
    public class QueryModule : NancyModule
    {
        IProjection<Entry> _entryRepository;
        IEventStore<Media> _mediaRepository;

        public QueryModule(ILogger<NancyModule> logger, IProjection<Entry> entryRepository, IEventStore<Media> mediaRepository)
        {
            _entryRepository = entryRepository;
            _mediaRepository = mediaRepository;

            Get("/micropub", p =>
            {
                string q = this.Request.Query["q"];

                switch (q)
                {
                    case "config":
                        return new Nancy.Responses.JsonResponse(new Config { MediaEndpoint = Path.Join(this.Request.Url.SiteBase, "/micropub/media") },
new Nancy.Responses.DefaultJsonSerializer(this.Context.Environment),
this.Context.Environment);
                    case "source":
                        return new Nancy.Responses.JsonResponse(_entryRepository.Get().OrderByDescending(i=>i.Published_At).Take(20).Select(i=>new { properties = i }),
new Nancy.Responses.DefaultJsonSerializer(this.Context.Environment),
this.Context.Environment);

                    default:
                        return "";

                }
            });
        }

    }
}
