using System;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;
using Microsoft.Extensions.Configuration;
using StrangeVanilla.Blogging.Events;
using SV.Maat.lib;

namespace SV.Maat.ExternalNetworks
{
    public class Twitter  : IRequiresOAuthRegistration
    {
        public string Name { get { return "Twitter"; } }
        public string Url{get { return "https://twitter.com";}}
        public string Photo { get { return ""; } }

        private readonly string ConsumerKey;
        private readonly string ConsumerKeySecret;

        public Twitter(IConfiguration config)
        {
            Dictionary<string,string> twitterConn = config
                .GetConnectionString("Twitter")
                ?.Split(";")
                ?.Select(x => x.Split('='))
                ?.ToDictionary(x => x[0].ToLower(), y => y[1]);

            if (!twitterConn.ContainsKey("consumerkey") || !twitterConn.ContainsKey("consumerkeysecret"))
            {
                throw new Exception("Twitter credentials missing or malformed");
            }

            ConsumerKey = twitterConn["consumerkey"];
            ConsumerKeySecret = twitterConn["consumerkeysecret"];
        }

        public Uri GetAuthorizeLink(string redirectUri)
        {
            var context = OAuth.Authorize(ConsumerKey, ConsumerKeySecret, redirectUri);
            
            return context.AuthorizeUri;
        }

        public Credentials GetToken(string redirectUri, string oauth_token, string oauth_verifier)
        {
            OAuth.OAuthSession context = new OAuth.OAuthSession() { RequestToken = oauth_token, ConsumerKey = ConsumerKey, ConsumerSecret = ConsumerKeySecret };
            var tokens =  context.GetTokens(oauth_verifier);
            
            return new Credentials { Uid = tokens.AccessToken, Secret = tokens.AccessTokenSecret };
        }

        public string GetProfileUrl(Credentials token)
        {

            Tokens tokens = new Tokens
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerKeySecret,
                AccessToken = token.Uid,
                AccessTokenSecret = token.Secret
            };
            var client = tokens.Account.VerifyCredentials();
            
            return $"{Url}/{client.ScreenName}";
        }

        public string Syndicate(Credentials credentials, Entry entry)
        {
            Tokens tokens = new Tokens
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerKeySecret,
                AccessToken = credentials.Uid,
                AccessTokenSecret = credentials.Secret
            };
            var client = tokens.Account.VerifyCredentials();

            var response = tokens.Statuses.Update(ContentHelper.GetPlainText(entry.Body));

            return $"{Url}/{client.ScreenName}/statuses/{response.Id}";

        }

    }
}
