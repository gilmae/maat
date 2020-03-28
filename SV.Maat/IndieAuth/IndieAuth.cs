using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace SV.Maat.IndieAuth
{
    /// <summary>
    /// A MVP implemnemtation of Access Token Verification
    ///
    /// TODO - everything else
    /// </summary>
    public class IndieAuth
    {
        private static string IndieAuthTokenValidationEndpoint =
           Environment.GetEnvironmentVariable("INDIE_AUTH_TOKEN_VALIDATION_ENDPOINT")
            ?? "https://tokens.indieauth.com/token";

        public IndieAuth()
        {
        }

        public static bool VerifyAccessToken(string access_token)
        {
            if (access_token.EndsWith("itsame")) // Dev mode
            {
                return true;
//                return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//{
//                        new Claim(ClaimTypes.Name, "test")}, "Basic"));
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
                var auth = JsonConvert.DeserializeObject<AccessTokenVerificationResponse>(body);

                if (new Uri(auth.me).Host != "avocadia.net" || !(auth.scope.Contains("create")))
                {
                    return false;
                }
                return true;
                //return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                //{
                //        new Claim(ClaimTypes.Name, auth.me)
                //}, "Basic"));
            }
        }
    }
}
