using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CoreTweet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        readonly Regex statusIdRegex = new Regex(@"^\/?[^\/]+\/{1}status\/{1}(\d+)");

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

        public Credentials GetToken(string redirectUri, IQueryCollection query)
        {
            string oauth_token = query["oauth_token"].FirstOrDefault();
            string oauth_verifier = query["oauth_verifier"].FirstOrDefault();

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

        public string Syndicate(Credentials credentials, Entry entry, string inNetworkReplyingTo = null)
        {
            Tokens tokens = new Tokens
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerKeySecret,
                AccessToken = credentials.Uid,
                AccessTokenSecret = credentials.Secret
            };
            var client = tokens.Account.VerifyCredentials();

            var media_ids = new long[] { };
            if (entry.AssociatedMedia != null)
            {
                media_ids = entry.AssociatedMedia.Select(m =>
                {
                    byte[] data = Downloader.Download(m.Url).Result;
    
                    var response = tokens.Media.Upload(data);
                    if (response != null && !string.IsNullOrEmpty(m.Description))
                    {
                        tokens.Media.Metadata.Create(response.MediaId.ToString(), m.Description);
                    }
                    return response?.MediaId ?? 0;
                }).Where(i => i != 0).ToArray();
            }


            var response = tokens.Statuses.Update(
                status: ContentHelper.GetPlainText(entry.Body),
                media_ids: media_ids,
                in_reply_to_status_id: GetStatusIdFromTweetUrl(inNetworkReplyingTo)
            );

            return $"{Url}/{client.ScreenName}/statuses/{response.Id}";
        }

        public bool IsUrlForNetwork(string url)
        {
            return GetStatusIdFromTweetUrl(url).HasValue;
        }

        private long? GetStatusIdFromTweetUrl(string url)
        {
            if (url.StartsWith(Url))
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    var path = new Uri(url).AbsolutePath;

                    var match = statusIdRegex.Match(path);

                    if (match != null && match.Groups != null && match.Groups.Count > 0)
                    {
                        if (long.TryParse(match.Groups[0].Value, out long id))
                        {
                            return id;
                        }
                    }
                }
            }
            return null;
        }
    }

    public static class ServicesExtensions
    {
        public static void AddTwitter(this IServiceCollection services)
        {
            services.AddTransient<ISyndicationNetwork, Twitter>();
        }
    }
}
