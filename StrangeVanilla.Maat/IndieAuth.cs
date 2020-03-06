using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using Nancy.Authentication.Stateless;
using Newtonsoft.Json;

namespace StrangeVanilla.Maat
{
    public class IndieAuth
    {
        private static string IndieAuthTokenValidationEndpoint =
           Environment.GetEnvironmentVariable("INDIE_AUTH_TOKEN_VALIDATION_ENDPOINT")
            ?? "https://tokens.indieauth.com/token";

        public class AuthData
        {
            public string me { get; set; }

            public string client_id { get; set; }

            public string scope { get; set; }
        }

        public static StatelessAuthenticationConfiguration GetAuthenticationConfiguration()
        {
            return new StatelessAuthenticationConfiguration(ctx => {

                string access_token = ctx.Request.Headers.Authorization;

                if (string.IsNullOrEmpty(access_token))
                {
                    string form_token = ctx.Request.Form["access_token"];
                    if (string.IsNullOrEmpty(form_token))
                    {
                        return null;
                    }

                    access_token = string.Concat("Bearer: ", ctx.Request.Form["access_token"]);
                }

                if (access_token.EndsWith("itsame")) // Dev mode
                {
                    return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
{
                        new Claim(ClaimTypes.Name, "test")}, "Basic"));
                }

                var req = WebRequest.Create(IndieAuthTokenValidationEndpoint);
                req.Method = "GET";
                req.Headers.Add("Content-Type", "application/json");
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Authorization", access_token);

                var response = req.GetResponse();

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string body = reader.ReadToEnd();
                    var auth = JsonConvert.DeserializeObject<AuthData>(body);

                    if (new Uri(auth.me).Host != "avocadia.net" || !(auth.scope.Contains("create")))
                    {
                        return null;
                    }

                    return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, auth.me)
                    }, "Basic"));
                }
            });
        }
    }
}
