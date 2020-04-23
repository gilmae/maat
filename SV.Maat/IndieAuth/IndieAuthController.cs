﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth
{
    [Route("auth")]
    public class IndieAuthController : Controller
    {
        const string DefaultScope = "create update delete query media";
        
        IAuthenticationRequestStore _authenticationRequestStore;
        IRepository<AccessToken> _accessTokenStore;
        MicroformatParser mfparser = new MicroformatParser();

        public IndieAuthController(IAuthenticationRequestStore authenticationRequestStore, IRepository<AccessToken> accessTokenStore)
        {
            _authenticationRequestStore = authenticationRequestStore;
            _accessTokenStore = accessTokenStore;
        }


        // Used for both AuthenticationRequest and AuthorizationRequest
        [HttpGet]
        public IActionResult AuthenticationRequest([FromQuery]AuthenticationRequest model)
        {
            if (string.IsNullOrEmpty(model.ResponseType))
            {
                model.ResponseType = "id";
            }

            if (model.ResponseType == "code" && string.IsNullOrEmpty(model.Scope))
            {
                model.Scope = DefaultScope;
            }

            model.ClientId = CanonicaliseUrl(model.ClientId);

            var app = GetApp(model.ClientId);
            if (app == null)
            {
                return BadRequest();
            }
            
            model.ClientLogo = app.Logo;
            model.ClientName = app.Name;

            model.AuthorisationCode = "";
            model.AuthCodeExpiresAt = DateTime.UtcNow;
            model.UserId =  this.Url.GetUserIdFromUrl(model.UserProfileUrl);

            _authenticationRequestStore.Insert(model);
            return View(model);
            
        }

        [HttpPost]
        public IActionResult VerifyAuthorisationCode([FromForm] AuthorisationCodeVerificationRequest model)
        {
            var request = _authenticationRequestStore.FindByAuthCode(model.AuthorisationCode);
            request.ClientId = CanonicaliseUrl(request.ClientId);

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

            return Ok(new { me = request.UserProfileUrl });
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
            request.ClientId = CanonicaliseUrl(request.ClientId);

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

            AccessToken token = new AccessToken {
                AuthenticationRequestId = request.id,
                UserId=this.Url.GetUserIdFromUrl(request.UserProfileUrl),
                Name = request.ClientName,
                Scope = request.Scope,
                ClientId = request.ClientId
            };
            _accessTokenStore.Insert(token);

            string access_token = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(System.Text.Json.JsonSerializer.Serialize(token))
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

        [HttpGet]
        [Route("token")]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public IActionResult VerifyTokenRequest()
        {
            var tokenIdClaim = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData && c.ValueType== "TokenId")?.Value;
            if (!int.TryParse(tokenIdClaim, out int tokenId))
            {
                return Unauthorized();
            }

            var token = _accessTokenStore.Find(tokenId);

            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                me = this.Url.ActionLink("view", "users", new { id = token.UserId }),
                scope = token.Scope,
                client_id = token.ClientId
            });

        }

        private string CanonicaliseUrl(string url)
        {
            return new Uri(url).ToString().ToLower();
        }

        private App GetApp(string clientId)
        {
            return mfparser.GetApps(clientId).Result.Where(a => a.Url == clientId).FirstOrDefault();
        } 
    }
}
