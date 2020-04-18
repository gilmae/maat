using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.Repository;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SV.Maat.IndieAuth
{
    [Route("auth")]
    public class IndieAuthController : Controller
    {

        IRepository<AuthenticationRequest> _authenticationRequestStore;

        public IndieAuthController(IRepository<AuthenticationRequest> authenticationRequestStore)
        {
            _authenticationRequestStore = authenticationRequestStore;
        }

        [HttpGet]
        public IActionResult AuthenticationRequest([FromQuery]AuthenticationRequest model)
        {
            if (string.IsNullOrEmpty(model.ResponseType))
            {
                model.ResponseType = "id";
            }
            _authenticationRequestStore.Insert(model);
            return View(model);
            
        }
    }
}
