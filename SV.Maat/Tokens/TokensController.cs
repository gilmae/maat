﻿using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV.Maat.IndieAuth;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib;
using SV.Maat.Users.Models;


namespace SV.Maat.Users
{
    [Route("token")]
    public class TokensController : Controller
    {
        IAccessTokenStore _accessTokenStore;
        TokenSigning _tokenSigning;

        public TokensController(IAccessTokenStore accessTokenStore, TokenSigning tokenSigning)
        {
            _accessTokenStore = accessTokenStore;
            _tokenSigning = tokenSigning;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Tokens()
        {
            int userId = this.UserId().GetValueOrDefault();
            var tokens = _accessTokenStore.FindByUser(userId).Select(x => new AccessTokenViewModel { Name = x.Name, Token = Convert.ToBase64String(
                    _tokenSigning.Encrypt(
                        System.Text.Encoding.ASCII.GetBytes(
                               System.Text.Json.JsonSerializer.Serialize(x)
                        )))
            });
            ViewBag.tokens = tokens;
            return View(tokens);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public ActionResult Create([FromForm] string name)
        {
            var token = new AccessToken
            {
                Name = name,
                UserId = this.UserId().Value,
                ClientId = "Manual",
                Scope = "create update delete query"
            };

            _accessTokenStore.Insert(token);

            return RedirectToAction("Tokens");
        }
    }
}