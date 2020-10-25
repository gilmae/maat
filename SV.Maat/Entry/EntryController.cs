using System;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.Projections;
using SV.Maat.lib;
using System.Linq;
using System.Collections.Generic;
using SV.Maat.ExternalNetworks;
using Users;
using StrangeVanilla.Blogging.Events;

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


        [Route("/")]
        public IActionResult Timeline()
        {
            var entries = _entries.Get().Where(x => x.Status == StrangeVanilla.Blogging.Events.StatusType.published).Take(20);

            var model = entries.Select(entry =>
                CreateModel(entry, _userStore.Find(entry.OwnerId))
            );

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

            var model = CreateModel(entry, user);
            return View(model);
        }

        private Models.Entry CreateModel(Entry entry, User user)
        {
            Models.Entry model = new Models.Entry
            {
                Title = entry.Title.GetPlainText(),
                Body = entry.Body.GetHtml(),
                Photos = entry.AssociatedMedia?.Where(m => m.Type == "photo").Select(m => new Models.Media { Url = m.Url, Description = m.Description }).ToArray() ?? new Models.Media[0],
                Categories = entry.Categories?.ToArray() ?? new string[0],
                PublishedAt = entry.PublishedAt.Value,
                Bookmark = entry.BookmarkOf,
                Url = UrlHelper.EntryUrl(entry, user)
            };
            model.AlternateVersions = new List<Models.AlternateVersion>().ToArray();
            if (!entry.Syndications.IsNull())
            {
                model.AlternateVersions = (from e in entry.Syndications
                                           join n in _externalNetworks on e.Network equals n.Name into gj
                                           from subnet in gj.DefaultIfEmpty()
                                           select new Models.AlternateVersion { Name = (subnet?.Name) ?? e.Url, Url = e.Url, Icon = (subnet?.Photo) ?? "" }).ToArray();

            }
            if (!entry.AssociatedMedia.IsNull() && entry.AssociatedMedia.Any(m => m.Type == MediaType.archivedCopy.ToString()))
            {
                model.AlternateVersions = model.AlternateVersions.Union(entry.AssociatedMedia?
                       .Where(m => m.Type == MediaType.archivedCopy.ToString())
                       .Select(m => new Models.AlternateVersion { Url = m.Url, Name = "Archive", Icon = "" })).ToArray();
            }
            return model;
        }
    }
}
