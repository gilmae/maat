using System;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;
using Microsoft.Extensions.Configuration;
using StrangeVanilla.Blogging.Events;

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

        public OAuthAccessToken GetToken(string redirectUri, string oauth_token, string oauth_verifier)
        {
            OAuth.OAuthSession context = new OAuth.OAuthSession() { RequestToken = oauth_token, ConsumerKey = ConsumerKey, ConsumerSecret = ConsumerKeySecret };
            var tokens =  context.GetTokens(oauth_verifier);
            
            return new OAuthAccessToken { AccessToken = tokens.AccessToken, AccessTokenSecret = tokens.AccessTokenSecret };
        }

        public string GetProfileUrl(OAuthAccessToken token)
        {

            Tokens tokens = new Tokens
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerKeySecret,
                AccessToken = token.AccessToken,
                AccessTokenSecret = token.AccessTokenSecret
            };
            var client = tokens.Account.VerifyCredentials();
            
            return $"{Url}/{client.ScreenName}";
        }

        public long Syndicate(OAuthAccessToken token, Entry entry)
        {
            Tokens tokens = new Tokens
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerKeySecret,
                AccessToken = token.AccessToken,
                AccessTokenSecret = token.AccessTokenSecret
            };
            var client = tokens.Account.VerifyCredentials();

            var response = tokens.Statuses.Update(entry.Body);

            return response.Id;

        }

    }

    public struct OAuthAccessToken
    {
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }

    public interface IRequiresOAuthRegistration : ISyndicationNetwork
    {
        Uri GetAuthorizeLink(string redirectUri);
        OAuthAccessToken GetToken(string redirectUri, string oauth_token, string oauth_verifier);
        string GetProfileUrl(OAuthAccessToken token);
    }

    public interface ISyndicationNetwork
    {
        string Name { get; }
        string Photo { get; }
        string Url { get; }
    }
}
