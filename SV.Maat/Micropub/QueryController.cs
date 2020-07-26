using System;
using System.Collections.Generic;
using System.Linq;
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
using SV.Maat.Projections;
using SV.Maat.ExternalNetworks;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public partial class QueryController : ControllerBase
    {
        IEntryProjection _entryView;
        ILogger<QueryController> _logger;
        readonly ISyndicationStore _syndicationStore;
        private readonly IEnumerable<ISyndicationNetwork> _externalNetworks;
        private readonly IRepliesProjection _repliesProjection;

        public QueryController(ILogger<QueryController> logger, IEntryProjection entryView, ISyndicationStore syndicationStore, IEnumerable<ISyndicationNetwork> externalNetworks, IRepliesProjection repliesProjection)
        {
            _logger = logger;
            _entryView = entryView;
            _syndicationStore = syndicationStore;
            _externalNetworks = externalNetworks;
            _repliesProjection = repliesProjection;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public IActionResult Query([FromQuery] QueryModel query)
        {
            string q = query.Query;
            if (q == QueryType.config)
            {
                return Ok(GetConfig());
            }
            else if(q == QueryType.syndicateTo)
            {
                return Ok(new SyndicateToModel { SyndicateTo = GetSupportedNetworks()?.ToArray() });
            }
            else if (q == QueryType.source)
            {
                return GetSourceQuery(query.Url, query.Properties, query.Limit, query.Before, query.After);
            }
            else if (q == QueryType.replies)
            {
                return GetReplies(query.Url);
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
                SupportedSyndicationNetworks = networksSupported?.ToArray()
            };
        }

        public IActionResult GetReplies(string url)
        {
            var replies = _repliesProjection.GetReplyIds(url);
            return Ok(new
            {
                replies = replies.Select(id => UrlHelper.EntryUrl(HttpContext, id))
            });

        }

        private IEnumerable<SupportedNetwork> GetSupportedNetworks()
        {
            IEnumerable<SupportedNetwork> networksSupported = null;
            int? userId = this.UserId();
            if (userId.HasValue)
            {
                networksSupported = from s in _syndicationStore.FindByUser(this.UserId() ?? -1)
                                    join n in _externalNetworks on s.Network.ToLower() equals n.Name.ToLower()
                                    select new SupportedNetwork
                                    {
                                        Name = $"{s.AccountName} on {n.Name}",
                                        Uid = string.Format(s.AccountName),
                                        Network = new NetworkDetails
                                        {
                                            Name = n.Name,
                                            Url = n.Url,
                                            Photo = n.Photo
                                        }
                                    };
            }

            return networksSupported;
        }

        private IActionResult GetSourceQuery(string url, string[] properties, int? limit, string before, string after)
        {
            if (!string.IsNullOrEmpty(url))
            {
                return GetSingleItem(url, properties);
            }
            else
            {
                return GetMulipleItems(properties, limit, before, after);
            }
        }

        private ActionResult GetMulipleItems(string[] properties, int? limit, string before, string after)
        {
            IEnumerable<Entry> entries = null;
            int pageSize = GetLimit(limit);
            if (!string.IsNullOrEmpty(before))
            {
                entries = _entryView.GetBefore(pageSize, before.FromBase64String<DateTime>());
            }
            else if (!string.IsNullOrEmpty(after))
            {
                entries = _entryView.GetAfter(pageSize, after.FromBase64String<DateTime>());
            }
            else
            {
                entries = _entryView.GetLatest(pageSize);
            }

            if (!entries.Any())
            {
                return Ok(new { items = entries.ToArray() });
            }

            EntryToMicropubConverter converter = new EntryToMicropubConverter(properties);

            var micropubEntries = entries.Select(e => converter.GetMicropub(e, entry=>HttpContext.EntryUrl(e)));

            return Ok(new
            {
                items = micropubEntries,
                paging = new
                {
                    before = entries.Last().CreatedAt.ToBase64String(),
                    after = entries.First().CreatedAt.ToBase64String()
                }
            });
        }

        private ActionResult GetSingleItem(string url, string[] properties)
        {
            Guid entryId = HttpContext.GetEntryIdFromUrl(url);
            if (entryId == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "The post with the requested URL was not found"
                });
            }

            Entry entry = _entryView.Get(entryId);
            if (entry == null)
            {
                return BadRequest(new {
                    error = "invalid_request",
                    error_description = "The post with the requested URL was not found"
                });
            }
            EntryToMicropubConverter converter = new EntryToMicropubConverter(properties);
            //return Ok(MicropubEnricher(entry, converter.ToDictionary(entry), false, properties.IsEmpty()));
            return Ok(converter.GetMicropub(entry, null));
        }

        private int GetLimit(int? limit)
        {
            int postLimit = 20;
            if (limit.HasValue)
            {
                postLimit = limit.Value;
            }
            if (postLimit < 1)
            {
                postLimit = 1;
            }
            else if (postLimit > 20)
            {
                postLimit = 20;
            }

            return postLimit;
        }

        
    }
}
