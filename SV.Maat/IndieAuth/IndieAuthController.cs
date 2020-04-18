using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
            model.AuthorisationCode = "";
            
            _authenticationRequestStore.Insert(model);
            return View(model);
            
        }

        [HttpPost]
        public IActionResult ApproveAuthenticationRequest([FromForm] int id)
        {
            AuthenticationRequest request = _authenticationRequestStore.Find(id);
            if(request == null)
            {
                return NotFound();
            }

            request.AuthorisationCode = Guid.NewGuid().ToString();
            request.AuthCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);

            _authenticationRequestStore.Update(request);


            UriBuilder uriBuilder = new UriBuilder(request.RedirectUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["code"] = request.AuthorisationCode;
            query["state"] = request.CsrfToken;

            uriBuilder.Query = query.ToString();

            return Redirect(uriBuilder.Uri.ToString());
        }
    }
}
