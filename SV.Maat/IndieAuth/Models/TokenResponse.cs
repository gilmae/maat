using System;
using Microsoft.AspNetCore.Mvc;

namespace SV.Maat.IndieAuth.Models
{
    public class TokenResponse
    {
        [BindProperty(Name = "scope")]
        public string Scope { get; set; }

        [BindProperty(Name = "me")]
        public string UserProfileUrl { get; set; }

        [BindProperty(Name ="access_token")]
        public string AccessToken { get; set; }

        [BindProperty(Name ="token_type")]
        public string TokenType { get; set; }
    }
}
