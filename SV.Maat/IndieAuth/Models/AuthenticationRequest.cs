﻿using System;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.lib.Repository;

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
    }
}