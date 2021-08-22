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
        IPostsProjection _entries;
        IEnumerable<ISyndicationNetwork> _externalNetworks;
        IUserStore _userStore;
        public EntryController(IPostsProjection entries,
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
            var entries = _entries.Get().Take(20);

            return Json(entries);
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
            
            return Json(entry);
        }
    }
}
