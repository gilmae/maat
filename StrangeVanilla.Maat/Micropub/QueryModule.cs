using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Events;
using Microsoft.Extensions.Logging;
using Nancy;
using StrangeVanilla.Blogging.Events;
using StrangeVanilla.Maat.lib;
using static StrangeVanilla.Blogging.Events.Entry;

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

                        var requestedProperties = (string[])MicropubBinder.AsArray(Request.Query["properties[]"]);
                        bool includeType = false;
                        bool includeUrl = true;
                        if (requestedProperties == null || requestedProperties.Count() == 0)
                        {
                            includeType = true;
                        }

                        var entries = _entryRepository
                            .Get();

                        string url = Request.Query["url"];
                        if (!string.IsNullOrEmpty(url))
                        {
                            includeUrl = false;
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
                            

                        var pagedEntries = entries.OrderByDescending(i => i.PublishedAt).Take(20);
                        EntryToMicropubConverter converter = new EntryToMicropubConverter(requestedProperties);


                        var micropubEntries = pagedEntries.Select(e => MicropubEnricher(Context, e, converter.ToDictionary(e), includeUrl, includeType));

                        
                            return new Nancy.Responses.JsonResponse(
                                micropubEntries,
                                new Nancy.Serialization.JsonNet.JsonNetSerializer(),
                                this.Context.Environment);
                            
                        

                    default:
                        return "";

                }
            });

            
        }

        private dynamic MicropubEnricher(NancyContext context, Entry entry, Dictionary<string,object> properties, bool includeUrl, bool includeType)
        {
            Dictionary<string, object> enrichedItem = new Dictionary<string, object>();
            enrichedItem["properties"] = properties;
            if (includeUrl)
            {
                enrichedItem["url"] = new[] { UrlHelper.EntryUrl(context, entry) };
            }
            if (includeType)
            {
                enrichedItem["type"] = "h-entry";
            }
            return enrichedItem;
        }
        

        
        private class EntryToMicropubConverter
        {
            private Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();
            private static Dictionary<string, string> columnMapper = new Dictionary<string, string>
            {
                {"name", "Title"},
                {"content", "Body" },
                {"category", "Categories" },
                {"photo", "AssociatedMedia" },
                {"in-reply-to", "ReplyTo" },
                {"post-status", "PublishedAt" },
                {"published", "PublishedAt" },
                {"bookmark-of", "BookmarkOf" }
            };

            public EntryToMicropubConverter() : this(null) { }
            public EntryToMicropubConverter(IList<string> columnsRequired)
            {
                if (columnsRequired == null || columnsRequired.Count == 0){
                    columnsRequired = columnMapper.Keys.ToList();
                }

                columnsRequired = columnsRequired.Where(c => columnMapper.ContainsKey(c)).ToList();

                Type entry = typeof(Entry);
                foreach (string column in columnsRequired)
                {
                    var property = entry.GetProperty(columnMapper[column]);
                    if (property != null)
                    {
                        _properties[column] = property;
                    }
                }
            }

            public Dictionary<string, object> ToDictionary(Entry entry)
            {
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (string key in _properties.Keys)
                {
                    values[key] = GetValue(key, entry);
                }

                return values;

            }


            public object GetValue(string columnName, Entry entry) 
            {
                object value = _properties[columnName].GetValue(entry);
                switch (columnName)
                {
                    case "name":
                    case "content":
                    case "in-reply-to":
                    case "bookmark-of":
                    case "category":
                    case "published":
                        return value;
                    case "photo":
                        var media = value as IList<MediaLink>;
                        if (media != null)
                        {
                            return media.Where(x => x.Type == "photo");
                        }
                        return null;
                    case "post-status":
                        return value == null ? "draft" : "published";
                    default:
                        return null;

                }



            }
        }
    }
}
