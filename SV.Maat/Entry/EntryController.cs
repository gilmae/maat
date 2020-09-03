using System;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.Projections;
using SV.Maat.lib;
using System.Linq;

namespace SV.Maat.Entries
{
    public class EntryController : Controller
    {
        IEntryProjection _entries;
        public EntryController(IEntryProjection entries)
        {
            _entries = entries;
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

            return View(model);
        }
    }
}
