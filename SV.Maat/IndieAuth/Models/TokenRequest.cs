using System;
using Microsoft.AspNetCore.Mvc;
using SimpleRepo;

namespace SV.Maat.IndieAuth.Models
{
    public class TokenRequest : Model
    {
        [BindProperty(Name = "client_id")]
        public string ClientId { get; set; }

        [BindProperty(Name = "redirect_uri")]
        public string RedirectUri { get; set; }

        [BindProperty(Name = "grant_type")]
        public string GrantType { get; set; }

        [BindProperty(Name ="me")]
        public string UserProfileUri { get; set; }

        [BindProperty(Name = "code")]
        public string AuthorisationCode { get; set; }
    }
}
