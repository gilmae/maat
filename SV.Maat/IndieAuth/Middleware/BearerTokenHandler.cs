﻿

using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SV.Maat.IndieAuth.Models;

namespace SV.Maat.IndieAuth.Middleware
{
    public class IndieAuthTokenHandler : AuthenticationHandler<IndieAuthOptions>
    {
        public const string SchemeName = "IndieAuth";

        public IndieAuthTokenHandler(
            IOptionsMonitor<IndieAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
           
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                AccessToken token = System.Text.Json.JsonSerializer.Deserialize<AccessToken>(Convert.FromBase64String(ReadToken()));

                var claims = new[]
                {
                    new Claim(ClaimTypes.Sid, token.UserId.ToString()),
                    new Claim(ClaimTypes.UserData, token.id.ToString(), "TokenId")
                };

            var claimsIdentity = new ClaimsIdentity(claims, nameof(IndieAuthTokenHandler));

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception Occured while Deserializing: " + ex);
                return Task.FromResult(AuthenticateResult.Fail("TokenParseException"));
            }
        }

        private string ReadToken()
        {
            string accessToken = string.Empty;
            if (Request.ContentType == "application/x-www-form-urlencoded" || Request.ContentType == "multipart/form-data")
            {
                accessToken = Request.Form["access-token"];
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = Request.Headers["Authorization"];
                accessToken = accessToken.Substring("Bearer ".Length); // Slice off the 'Bearer ' prefix
            }

            return accessToken;
        }
    }

    public class IndieAuthOptions : AuthenticationSchemeOptions { }
}
