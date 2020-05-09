using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SV.Maat.IndieAuth.Middleware;
using SV.Maat.Syndications.Models;

namespace SV.Maat.Syndications
{
    [Route("syndication")]
    public class SyndicationController : ControllerBase
    {
        readonly ISyndicationStore _syndicationStore;
        private readonly SyndicationNetworks _networks;

        public SyndicationController(ISyndicationStore syndicationStore, IOptions<SyndicationNetworks> networkOptions)
        {
            _syndicationStore = syndicationStore;
            _networks = networkOptions.Value; 
        }

        [HttpGet]
        [Route("networks")]
        public ActionResult ListNetworks()
        {
            return Ok(_networks.Networks.Keys);
        }

        [HttpPost]
        [Route("register")]
        [Authorize(AuthenticationSchemes = IndieAuthTokenHandler.SchemeName)]
        public ActionResult Register ([FromBody] RegisterSyndicationModel model)
        {
            int userId = int.Parse(this.User.Claims.First(c => c.Type == ClaimTypes.Sid)?.Value);
            Syndication syndication = new Syndication { AccountName = model.Name, Network = model.Network.ToString(), UserId=userId };
            _syndicationStore.Insert(syndication);

            return Ok();
        }

        
    }
}
