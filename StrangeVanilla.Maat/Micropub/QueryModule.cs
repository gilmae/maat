using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Events;
using FastMember;
using Microsoft.Extensions.Logging;
using Nancy;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Maat.lib;

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
                        var accessor = TypeAccessor.Create(typeof(MicropubPost));

                        var test = new MicropubPost();
                        var testAccessor = ObjectAccessor.Create(new MicropubPost());

                        var requestedProperties = SanitiseColumns((string[])MicropubBinder.AsArray(Request.Query["properties[]"]))
                        .Where(p=> testAccessor.IsDefined(p.Key));

                        var entries = _entryRepository
                            .Get();

                        string url = Request.Query["url"];
                        if (!string.IsNullOrEmpty(url))
                        {
                            Guid entryId = this.Context.GetEntryIdFromUrl(url);

                            if (entryId == Guid.Empty)
                            {
                                entries = entries.Where(e => e.Syndications.Contains(url)); 
                            }
                            else
                            {
                                entries = entries.Where(e => e.Id == entryId);
                            }
                        }
                            

                        var filteredEntries = entries.OrderByDescending(i => i.Published_At).Take(20)
                            .Select(i => new MicropubPost { Type = "Entry",
                                Title = i.Title,
                                Content = i.Body,
                                Categories = i.Categories?.ToArray(),
                                BookmarkOf = i.BookmarkOf });

                        if (requestedProperties != null & requestedProperties.Count() > 0)
                        {
                            var properties = entries.Select(i => GetColumns(requestedProperties, i, accessor));
                            return new Nancy.Responses.JsonResponse(
                                new {
                                    properties
                                },
                                new Nancy.Serialization.JsonNet.JsonNetSerializer(),
                                this.Context.Environment);
                            
                        }

                        return new Nancy.Responses.JsonResponse(new { properties = entries },
                                 new Nancy.Serialization.JsonNet.JsonNetSerializer(),
                                 this.Context.Environment);

                    default:
                        return "";

                }
            });
        }

        private Dictionary<string,string> SanitiseColumns(string[] columns)
        {
            Dictionary<string, string> columnMapping = new Dictionary<string, string>();

            foreach (string c in columns)
            {
                switch (c)
                {
                    case "h":
                        columnMapping["Type"] = c;
                        break;

                    case "content":
                        columnMapping["Content"] = c;
                        break;

                    case "name":
                        columnMapping["Title"] = c;
                        break;

                    case "category":
                        columnMapping["Categories"] = c;
                        break;

                    case "post-status":
                        columnMapping["PostStatus"] = c;
                        break;

                    case "bookmark-of":
                        columnMapping["BookmarkOf"] = c;
                        break;
                    default:
                        columnMapping[c] = c;
                        break;
                }
            }

            return columnMapping;
        }

        private Dictionary<string, object> GetColumns(IEnumerable<KeyValuePair<string,string>> columns, object source, TypeAccessor accessor)
        {
            return columns.ToDictionary(i => i.Value, i => accessor[source, i.Key]);
        }

    }
}
