using System;
using Microsoft.AspNetCore.Mvc;

namespace SV.Maat.IndieAuth.Models
{
    public class AuthorisationCodeVerificationRequest
    {
        [BindProperty(Name = "code")]
        public string AuthorisationCode { get; set; }

        [BindProperty(Name = "client_id")]
        public string ClientId { get; set; }

        [BindProperty(Name = "redirect_uri")]
        public string RedirectUri { get; set; }
    }
}
