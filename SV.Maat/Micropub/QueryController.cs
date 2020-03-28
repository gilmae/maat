using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StrangeVanilla.Blogging.Events;
using SV.Maat.lib;
using SV.Maat.Micropub.Models;
using static StrangeVanilla.Blogging.Events.Entry;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public class QueryController : ControllerBase
    {
        IProjection<Entry> _entryView;
        ILogger<QueryController> _logger;

        public QueryController(ILogger<QueryController> logger, IProjection<Entry> entryView)
        {
            _logger = logger;
            _entryView = entryView;
        }

        [HttpGet]
        public IActionResult Query([FromQuery] QueryModel query)
        {
            if (!IndieAuth.IndieAuth.VerifyAccessToken(Request.Headers["Authorization"]))
            {
                return Unauthorized();
            }
            QueryType q = (QueryType)Enum.Parse(typeof(QueryType), query.Query);
            if (q == QueryType.config)
            {
                return Ok(GetConfig());
            }
            else if (q == QueryType.source)
            {
                return GetSourceQuery(query.Url, query.Properties);
            }
            return Ok();
        }

        private Config GetConfig()
        {
            return new Config { MediaEndpoint = Url.ActionLink("CreateMedia", "Micropub") };
        }

        private IActionResult GetSourceQuery(string url, string[] properties)
        {
            bool includeType = false;
            bool includeUrl = true;
            if (properties == null || properties.Count() == 0)
            {
                includeType = true;
            }

            var entries = _entryView.Get();

            if (!string.IsNullOrEmpty(url))
            {
                includeUrl = false;
                Guid entryId = HttpContext.GetEntryIdFromUrl(url);

                if (entryId == Guid.Empty)
                {
                    entries = entries.Where(e => (e.Syndications?.Contains(url)).GetValueOrDefault());
                }
                else
                {
                    entries = entries.Where(e => e.Id == entryId);
                }
            }

            var pagedEntries = entries.OrderByDescending(i => i.PublishedAt).Take(20);
            EntryToMicropubConverter converter = new EntryToMicropubConverter(properties);

            var micropubEntries = pagedEntries.Select(e => MicropubEnricher(e, converter.ToDictionary(e), includeUrl, includeType));

            return Ok(micropubEntries);
        }

        private dynamic MicropubEnricher(Entry entry, Dictionary<string, object> properties, bool includeUrl, bool includeType)
        {
            Dictionary<string, object> enrichedItem = new Dictionary<string, object>();
            enrichedItem["properties"] = properties;
            if (includeUrl)
            {
                enrichedItem["url"] = new[] { HttpContext.EntryUrl(entry) };
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
                if (columnsRequired == null || columnsRequired.Count == 0)
                {
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
