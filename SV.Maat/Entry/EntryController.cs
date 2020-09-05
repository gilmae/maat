using System;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.Projections;
using SV.Maat.lib;
using System.Linq;
using System.Collections.Generic;
using SV.Maat.ExternalNetworks;

namespace SV.Maat.Entries
{
    public class EntryController : Controller
    {
        IEntryProjection _entries;
        IEnumerable<ISyndicationNetwork> _externalNetworks;
        public EntryController(IEntryProjection entries,
            IEnumerable<ISyndicationNetwork> externalNetworks)
        {
            _entries = entries;
            _externalNetworks = externalNetworks;
        }

        [HttpGet]
        [Route("/entries/{id}")]
        public IActionResult Entry([FromRoute] Guid id)
        {
            var entry = _entries.Get(id, true);
            if (entry == null)
            {
                return NotFound();
            }

            Models.Entry model = new Models.Entry
            {
                Title = entry.Title.GetPlainText(),
                Body = entry.Body.GetHtml(),
                Photos = entry.AssociatedMedia?.Select(m => new Models.Media { Url = m.Url, Description = m.Description }).ToArray() ?? new Models.Media[0],
                Categories = entry.Categories?.ToArray() ?? new string[0],
                PublishedAt = entry.PublishedAt.Value,
                Bookmark = entry.BookmarkOf
            };
            
            if (!entry.Syndications.IsNull())
            {
                model.AlternateVersions = (from e in entry.Syndications
                                          join n in _externalNetworks on e.Network equals n.Name
                                          select new Models.AlternateVersion { Name = n.Name??"Unknown", Url = e.Url, Icon = n.Photo??"" }).ToArray();

            }

            return View(model);
        }
    }
}
