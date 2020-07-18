using System;
using Microsoft.AspNetCore.Mvc;
using SimpleRepo;

namespace SV.Maat.IndieAuth.Models
{
    public class AuthenticationRequest : Model
    {
        [BindProperty(Name = "me")]
        public string UserProfileUrl { get; set; }

        [BindProperty(Name ="client_id")]
        public string ClientId { get; set; }

        [BindProperty(Name = "redirect_uri")]
        public string RedirectUri { get; set; }

        [BindProperty(Name = "state")]
        public string CsrfToken { get; set; }

        [BindProperty(Name="response_type")]
        public string ResponseType { get; set; }

        [BindProperty(Name ="scope")]
        public string Scope { get; set; }

        public string ClientName { get; set; }
        public string ClientLogo { get; set; }

        public string AuthorisationCode { get; set; }

        public DateTime? AuthCodeExpiresAt { get; set; }

        public string Status { get; set; }

        public string AccessToken { get; set; }

        public int UserId { get; set; }
    }
}
