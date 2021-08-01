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
using Users;

namespace SV.Maat.Micropub
{
    [ApiController]
    [Route("micropub")]
    public partial class QueryController : ControllerBase
    {
        IPostsProjection _entryView;
        ILogger<QueryController> _logger;
        readonly ISyndicationStore _syndicationStore;
        private readonly IEnumerable<ISyndicationNetwork> _externalNetworks;
        private readonly IRepliesProjection _repliesProjection;
        IUserStore _userStore; 

        public QueryController(ILogger<QueryController> logger,
            IPostsProjection postsView,
            ISyndicationStore syndicationStore,
            IEnumerable<ISyndicationNetwork> externalNetworks,
            IRepliesProjection repliesProjection,
            IUserStore userStore)
        {
            _logger = logger;
            _entryView = postsView;
            _syndicationStore = syndicationStore;
            _externalNetworks = externalNetworks;
            _repliesProjection = repliesProjection;
            _userStore = userStore;
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
                replies = replies.Select(id => {
                    var e = _entryView.Get(id);
                    var u = _userStore.Find(e.OwnerId);

                    return UrlHelper.PostUrl(e, u);

                })
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
            IEnumerable<Post> posts = null;
            List<string> propertiesToInclude = new List<string>();
            if (!properties.IsEmpty())
            {
                propertiesToInclude.AddRange(properties);
            }


            int pageSize = GetLimit(limit);
            if (!string.IsNullOrEmpty(before))
            {
                posts = _entryView.GetBefore(pageSize, before.FromBase64String<DateTime>());
            }
            else if (!string.IsNullOrEmpty(after))
            {
                posts = _entryView.GetAfter(pageSize, after.FromBase64String<DateTime>());
            }
            else
            {
                posts = _entryView.GetLatest(pageSize);
            }

            if (!posts.Any())
            {
                return Ok(new { items = new object[] { } });
            }

            return Ok(new
            {
                items = posts.Select(i => new
                {
                    type = i.Data.Type,
                    properties = i.Data.Properties.Where(kv => propertiesToInclude.IsEmpty() || propertiesToInclude.Contains(kv.Key)),
                    children = i.Data.Children,
                    url = UrlHelper.PostUrl(i, _userStore.Find(i.OwnerId))
                }),
                paging = new
                {
                    before = posts.Last().CreatedAt.ToBase64String(),
                    after = posts.First().CreatedAt.ToBase64String()
                }
            });
        }

        private ActionResult GetSingleItem(string url, string[] properties)
        {
            Post entry = _entryView.Get(new Uri(url)?.AbsolutePath);
            List<string> propertiesToInclude = new List<string>();
            if (!properties.IsEmpty())
            {
                propertiesToInclude.AddRange(properties);
            }
            if (entry == null)
            {
                return BadRequest(new {
                    error = "invalid_request",
                    error_description = "The post with the requested URL was not found"
                });
            }
            return Ok(new
            {
                type = entry.Data.Type,
                properties = entry.Data.Properties.Where(kv => propertiesToInclude.IsEmpty() || propertiesToInclude.Contains(kv.Key)),
                children = entry.Data.Children,
                url = UrlHelper.PostUrl(entry, _userStore.Find(entry.OwnerId))
            });
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
