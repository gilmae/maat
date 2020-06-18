using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SV.Maat.ExternalNetworks;
using SV.Maat.IndieAuth;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.Syndications.Models;

namespace SV.Maat.Syndications
{
    [Route("syndication")]
    public class SyndicationsController : Controller
    {
        readonly ISyndicationStore _syndicationStore;
        private readonly TokenSigning _tokenSigning;
        IEnumerable<ISyndicationNetwork> _externalNetworks;

        public SyndicationsController(ISyndicationStore syndicationStore,
            TokenSigning tokenSigning,
            IEnumerable<ISyndicationNetwork> externalNetworks
        )
        {
            _syndicationStore = syndicationStore;
            _tokenSigning = tokenSigning;
            _externalNetworks = externalNetworks;
        }

        [HttpGet]
        [Route("networks")]
        public ActionResult ListNetworks()
        {
            return Ok(_externalNetworks.Select(n =>n.Name));
        }

        [HttpGet]
        [Route("register")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Register()
        {
            ViewBag.networks = _externalNetworks.Select(n => new SyndicationNetwork { name = n.Name, url=n.Url, photo=n.Photo }).ToList();
            return View();
        }

        [HttpPost]
        [Route("register")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Register([FromForm] RegisterSyndicationModel model)
        {
            int userId = int.Parse(this.User.Claims.First(c => c.Type == ClaimTypes.Sid)?.Value);
            Syndication syndication = new Syndication { AccountName = "Pending", Network = model.Network.ToString(), UserId = userId };
            var id = _syndicationStore.Insert(syndication);

            var network = _externalNetworks.First(n => n.Name.ToLower() == model.Network.ToString().ToLower());

            if (network is IRequiresFederatedInstance)
            {
                return new RedirectToActionResult("EnterInstance", "Syndications", new { id });
            }
            else if (network is IRequiresOAuthRegistration)
            {
                var returnUrl = (network as IRequiresOAuthRegistration).GetAuthorizeLink(Url.ActionLink("CompleteRegistration", "Syndications", new { id })).AbsoluteUri;
                return new RedirectResult(returnUrl);
            }
            else if (network is IRequiresCredentialEntry)
            {
                return new RedirectResult(Url.ActionLink("EnterCredentials", "Syndications", new { id }));
            }

            return Ok();
            
        }

        [HttpGet]
        [Route("register/complete")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult CompleteRegistration([FromQuery] int id, [FromQuery] string oauth_token, [FromQuery] string oauth_verifier)
        {
            Syndication syndication = _syndicationStore.Find(id);
            var network = _externalNetworks.First(n => n.Name.ToLower() == syndication.Network.ToLower()) as IRequiresOAuthRegistration;

            var tokens = network.GetToken(Url.ActionLink("CompleteRegistration", "Syndications", new { id }), oauth_token, oauth_verifier);

            Credentials credentials = new Credentials();
            if (!string.IsNullOrEmpty(syndication.Credentials))
            {
                credentials = _tokenSigning.Decrypt<Credentials>(syndication.Credentials);
            }

            credentials.Uid = tokens.Uid;
            credentials.Secret = tokens.Secret;

            syndication.Credentials = _tokenSigning.Encrypt(credentials);
            syndication.AccountName = network.GetProfileUrl(tokens);

            _syndicationStore.Update(syndication);

            return Redirect(Url.ActionLink("Register"));
        }

        [HttpGet]
        [Route("register/instance")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult EnterInstance([FromQuery] int id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        [Route("register/instance")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult EnterInstance([FromForm] int id, [FromForm] string instanceName)
        {
            Syndication syndication = _syndicationStore.Find(id);

            syndication.Credentials = _tokenSigning.Encrypt(new Credentials { Instance = instanceName });
            
            _syndicationStore.Update(syndication);

            var network = _externalNetworks.First(n => n.Name.ToLower() == syndication.Network.ToLower()) as IRequiresOAuthRegistration;

            if (network is IRequiresOAuthRegistration)
            {
                var returnUrl = network.GetAuthorizeLink(Url.ActionLink("CompleteRegistration", "Syndications", new { id })).AbsoluteUri;
                return new RedirectResult(returnUrl);
            }
            else if (network is IRequiresCredentialEntry)
            {
                return new RedirectResult(Url.ActionLink("EnterCredentials", "Syndications", new { id }));
            }

            return Ok();
        }

        [HttpGet]
        [Route("register/credentials")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult EnterCredentials([FromQuery] int id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        [Route("register/credentials")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult EnterCredentials([FromForm] int id, [FromForm] string name, [FromForm] string uid, [FromForm] string secret)
        {
            Syndication syndication = _syndicationStore.Find(id);

            var credentials = new Credentials { Uid = uid, Secret = secret };
            syndication.Credentials = _tokenSigning.Encrypt(credentials);
            syndication.AccountName = name;

            _syndicationStore.Update(syndication);

            return Redirect(Url.ActionLink("Register"));
        }
    }
}
