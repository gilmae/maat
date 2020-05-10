using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrangeVanilla.Blogging.Events;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.lib;
using SV.Maat.Micropub.Models;
using SV.Maat.Syndications;
using SV.Maat.Syndications.Models;
using static StrangeVanilla.Blogging.Events.Entry;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public class QueryController : ControllerBase
    {
        IProjection<Entry> _entryView;
        ILogger<QueryController> _logger;
        readonly ISyndicationStore _syndicationStore;
        private readonly SyndicationNetworks _networks;

        public QueryController(ILogger<QueryController> logger, IProjection<Entry> entryView, ISyndicationStore syndicationStore, IOptions<SyndicationNetworks> networkOptions)
        {
            _logger = logger;
            _entryView = entryView;
            _syndicationStore = syndicationStore;
            _networks = networkOptions.Value;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public IActionResult Query([FromQuery] QueryModel query)
        {
            //if (!IndieAuth.IndieAuth.VerifyAccessToken(Request.Headers["Authorization"]))
            //{
            //    return Unauthorized();
            //}
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
            IEnumerable<SupportedNetwork> networksSupported = GetSupportedNetworks();

            return new Config
            {
                MediaEndpoint = Url.ActionLink("CreateMedia", "Micropub"),
                SupportedQueries = new[] { "config", "source" },
                SupportedSyndicationNetworks = networksSupported.ToArray()
            };
        }

        private IEnumerable<SupportedNetwork> GetSupportedNetworks()
        {
            IEnumerable<SupportedNetwork> networksSupported = null;
            int? userId = this.UserId();
            if (userId.HasValue)
            {
                networksSupported = from s in _syndicationStore.FindByUser(this.UserId() ?? -1)
                                    join n in _networks.Networks on s.Network equals n.Key
                                    select new SupportedNetwork
                                    {
                                        Name = $"{s.AccountName} on {n.Value.name}",
                                        Uid = string.Format(n.Value.uidformat, s.AccountName),
                                        Network = new NetworkDetails
                                        {
                                            Name = n.Value.name,
                                            Url = n.Value.url,
                                            Photo = n.Value.photo
                                        }
                                    };
            }

            return networksSupported;
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
                {"post-status", "Status" },
                {"published", "PublishedAt" },
                {"bookmark-of", "BookmarkOf" },
                { "dt-deleted", "DeletedAt"}
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
                    case "dt-deleted":
                        return value;
                    case "photo":
                        var media = value as IList<MediaLink>;
                        if (media != null)
                        {
                            return media.Where(x => x.Type == "photo");
                        }
                        return null;
                    case "post-status":
                        return value.ToString();
                    default:
                        return null;

                }
            }
        }
    }
}
