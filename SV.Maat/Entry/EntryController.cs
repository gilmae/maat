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
                Body = entry.Body.GetPlainText(),
                Photos = entry.AssociatedMedia.Select(m => m.Url).ToArray(),
                Categories = entry.Categories.ToArray(),
                PublishedAt = entry.PublishedAt.Value
            };

            return View(model);
        }
    }
}
