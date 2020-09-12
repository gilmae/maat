using System;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.Projections;
using SV.Maat.lib;
using System.Linq;
using System.Collections.Generic;
using SV.Maat.ExternalNetworks;
using Users;

namespace SV.Maat.Entries
{
    public class EntryController : Controller
    {
        IEntryProjection _entries;
        IEnumerable<ISyndicationNetwork> _externalNetworks;
        IUserStore _userStore;
        public EntryController(IEntryProjection entries,
            IEnumerable<ISyndicationNetwork> externalNetworks,
            IUserStore userStore)
        {
            _entries = entries;
            _externalNetworks = externalNetworks;
            _userStore = userStore;
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Timeline()
        {
            var entries = _entries.Get().Where(x => x.Status == StrangeVanilla.Blogging.Events.StatusType.published).Take(20);

            var model = entries.Select(entry =>
            {
                var m = new Models.Entry
                {
                    Title = entry.Title.GetPlainText(),
                    Body = entry.Body.GetHtml(),
                    Photos = entry.AssociatedMedia?.Select(m => new Models.Media { Url = m.Url, Description = m.Description }).ToArray() ?? new Models.Media[0],
                    Categories = entry.Categories?.ToArray() ?? new string[0],
                    PublishedAt = entry.PublishedAt.Value,
                    Bookmark = entry.BookmarkOf,
                    Url = UrlHelper.EntryUrl(entry, _userStore.Find(entry.OwnerId))
                };
                if (!entry.Syndications.IsNull())
                {
                    m.AlternateVersions = (from e in entry.Syndications
                                           join n in _externalNetworks on e.Network equals n.Name into gj
                                           from subnet in gj.DefaultIfEmpty()
                                           select new Models.AlternateVersion { Name = (subnet?.Name) ?? e.Url, Url = e.Url, Icon = (subnet?.Photo) ?? "" }).ToArray();

                }
                return m;
            });

            return View(model.ToList());
        }

        [HttpGet]
        [Route("/entries/{id}")]
        public IActionResult Entry([FromRoute] Guid id)
        {
            var entry = _entries.Get(id);
            if (entry == null)
            {
                return NotFound();
            }
            var user = _userStore.Find(entry.OwnerId);
            Models.Entry model = new Models.Entry
            {
                Title = entry.Title.GetPlainText(),
                Body = entry.Body.GetHtml(),
                Photos = entry.AssociatedMedia?.Select(m => new Models.Media { Url = m.Url, Description = m.Description }).ToArray() ?? new Models.Media[0],
                Categories = entry.Categories?.ToArray() ?? new string[0],
                PublishedAt = entry.PublishedAt.Value,
                Bookmark = entry.BookmarkOf,
                Url = UrlHelper.EntryUrl(entry, user)
            };

            if (!entry.Syndications.IsNull())
            {
                model.AlternateVersions = (from e in entry.Syndications
                                           join n in _externalNetworks on e.Network equals n.Name into gj
                                           from subnet in gj.DefaultIfEmpty()
                                           select new Models.AlternateVersion { Name = (subnet?.Name) ?? e.Url, Url = e.Url, Icon = (subnet?.Photo) ?? "" }).ToArray();

            }

            return View(model);
        }
    }
}
