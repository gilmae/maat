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

        IAuthenticationRequestStore _authenticationRequestStore;

        public IndieAuthController(IAuthenticationRequestStore authenticationRequestStore)
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
            model.AuthCodeExpiresAt = DateTime.UtcNow;
            
            _authenticationRequestStore.Insert(model);
            return View(model);
            
        }

        [HttpPost]
        public IActionResult VerifyAuthorisationCode([FromForm] AuthorisationCodeVerificationRequest model)
        {
            var request = _authenticationRequestStore.FindByAuthCode(model.AuthorisationCode);
            if (request == null)
            {
                return BadRequest();
            }

            if (model.ClientId != request.ClientId || model.RedirectUri != request.RedirectUri)
            {
                return BadRequest();
            }

            if (request.AuthCodeExpiresAt < DateTime.UtcNow)
            {
                return BadRequest();
            }

            return Ok(new { me = this.Url.ActionLink("View", "Users", new { id = 1 }) });


        }

        [HttpPost]
        [Route("approve")]
        public IActionResult ApproveAuthenticationRequest([FromForm] int id, string action)
        {
            AuthenticationRequest request = _authenticationRequestStore.Find(id);
            if(request == null)
            {
                return NotFound();
            }

            request.State = action;
            if (action == "reject")
            {
                _authenticationRequestStore.Update(request);
                return Ok();
            }

            request.AuthorisationCode = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(Guid.NewGuid().ToString()));
            request.AuthCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);

            _authenticationRequestStore.Update(request);


            UriBuilder uriBuilder = new UriBuilder(request.RedirectUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["code"] = request.AuthorisationCode;
            query["state"] = request.CsrfToken;

            uriBuilder.Query = query.ToString();

            return Redirect(uriBuilder.Uri.ToString());
        }

        [HttpPost]
        [Route("token")]
        public IActionResult TokenRequest([FromForm]TokenRequest model)
        {
            var request = _authenticationRequestStore.FindByAuthCode(model.AuthorisationCode);
            if (request == null)
            {
                return BadRequest();
            }

            if (model.ClientId != request.ClientId || model.RedirectUri != request.RedirectUri)
            {
                return BadRequest();
            }

            if (request.AuthCodeExpiresAt < DateTime.UtcNow)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(request.Scope))
            {
                return BadRequest();
            }
            BearerToken token = new BearerToken { AuthenticationRequest = request.id };

            string access_token = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    System.Text.Json.JsonSerializer.Serialize(token)
                    )
                );

            _authenticationRequestStore.Update(request);

            return Ok(new TokenResponse
            {
                AccessToken = access_token,
                UserProfileUrl = request.UserProfileUrl,
                Scope = request.Scope,
                TokenType = "Bearer"
            });
        }
    }
}
