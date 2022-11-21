using System;
using StrangeVanilla.Blogging.Events;
using SV.Maat.ExternalNetworks;
using SV.Maat.IndieAuth;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using SV.Maat.lib;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.RegularExpressions;

namespace SV.Maat.Mastodon
{
    public class Mastodon : IRequiresOAuthRegistration, IRequiresFederatedInstance
    {
        private IMastodonAppStore _appStore;
        private TokenSigning _tokenSigning;
        public Mastodon(IMastodonAppStore mastodonAppStore, TokenSigning tokenSigning)
        {
            _appStore = mastodonAppStore;
            _tokenSigning = tokenSigning;
        }

        public string Name => "Mastodon";

        public string Photo => "https://joinmastodon.org/static/media/logo_full.97822390.svg";

        public string Url => "https://joinmastodon.org";

        readonly Regex statusIdRegex = new Regex(@"^\/?@{1}[^\/]+\/?(\d+)");
        readonly Regex statusIdRegexLong = new Regex(@"^\/?users\/{ 1 }[^\/]+\/{1}statuses\/{1}(\d+)");

        public MastodonApp App { get; set; }

        public Uri GetAuthorizeLink(string redirectUri)
        {
            if (App == null)
            {
                throw new ArgumentException("Mastodon Application not set.");
            }

            AppRegistration registration = _tokenSigning.Decrypt<AppRegistration>(App.authenticationclient);

            return new Uri($"{App.instance}/oauth/authorize?response_type=code&client_id={registration.client_id}&redirect_uri={redirectUri}&scope=read+write+follow+push");
        }

        public string GetProfileUrl(Credentials credentials)
        {
            var account = GetAccount(credentials.Secret);

            return account.Url.ToString();
        }

        public Credentials GetToken(string redirectUri, IQueryCollection query)
        {
            string code = query["code"].FirstOrDefault();

            if (App == null)
            {
                throw new ArgumentException("Mastodon Application not set.");
            }

            AppRegistration registration = _tokenSigning.Decrypt<AppRegistration>(App.authenticationclient);

            var token = GetTokenFromInstance(App.instance, code, registration.client_id, registration.client_secret, redirectUri);

            return new Credentials { Instance = App.instance, Secret = token.access_token };
        }

        public bool RegisterApp(string instance, string name, string url, string returnUrls)
        {
            var app = _appStore.FindByInstance(instance);
            if (app == null)
            {
                var registration = RegisterAppOnInstance(instance, name, returnUrls, url);
                app = new MastodonApp { instance = instance, authenticationclient = _tokenSigning.Encrypt(registration) };
                _appStore.Insert(app);
            }

            App = app;

            return true;
        }

        public bool SetInstance(string instance)
        {
            App = _appStore.FindByInstance(instance);


            return App != null;
        }

        public string Syndicate(Credentials credentials, Entry entry, string inNetworkReplyingTo = null)
        {
            if (App == null)
            {
                SetInstance(credentials.Instance);
                if (App == null)
                {
                    throw new ArgumentException("Mastodon Application not set.");
                }
            }

            var client = new RestClient(App.instance);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(credentials.Secret, "Bearer");

            IList<long> media_ids = new List<long>();
            if (entry.AssociatedMedia != null)
            {
                foreach (var m in entry.AssociatedMedia)
                {
                    var media_request = new RestRequest("api/v2/media");
                    byte[] data = Downloader.Download(m.Url).Result;
                    var filename = $"{Guid.NewGuid().ToString()}.{MimeTypes.MimeTypeMap.GetExtension(m.Type)}";
                    media_request.AddFileBytes("file", data, filename, m.Type);
                    media_request.AddParameter("description", m.Description);
                    var media_response = client.Post<Attachment>(media_request);

                    if (media_response==null || media_response.Data == null)
                    {
                        continue;
                    }
                    long id = media_response.Data.Id;

                    if (media_response.StatusCode == HttpStatusCode.OK)
                    {
                        media_ids.Add(id);
                    }
                    // If we get a 202 then Mastodon is still trying to process the file
                    // So we'll go into a loop and check for when it is accepted.
                    // Sleep for a few seconds to give Mastodon some time and then GET the file
                    // If we get a 200, then processing is done and we can proceed
                    // Otherwise we increment the try_count and go again
                    // The sleep is ever increasing
                    // Once we get to 5 attempts, just give it up, it's not going to work
                    else if (media_response.StatusCode == HttpStatusCode.Accepted)
                    {
                        var try_count = 1;
                        var status_check_request = new RestRequest($"api/v2/media/{id}");
                        while (try_count < 6) {
                            System.Threading.Thread.Sleep(try_count * 10000);
                            var status_check_response = client.Get(status_check_request);
                            if (status_check_response.StatusCode == HttpStatusCode.OK)
                            {
                                media_ids.Add(id);
                                break;
                            }
                            try_count += 1;
                        }
                    }
                }
            }

            var request = new RestRequest("api/v1/statuses")
                .AddJsonBody(new
                {
                    status = ContentHelper.GetPlainText(entry.Body),
                    media_ids,
                    in_reply_to_id = GetStatusIdFromTootUrl(inNetworkReplyingTo)
                });
            var response = client.Post<Status>(request);
            return response?.Data?.Url?.ToString();
        }

        private AppRegistration RegisterAppOnInstance(string instance, string name, string returnurls, string url)
        {
            var client = new RestClient(instance);
            var request = new RestRequest("api/v1/apps", DataFormat.Json)
                .AddJsonBody(new
                {
                    client_name = name,
                    redirect_uris = returnurls,
                    website = url,
                    scopes = "read write follow push"
                });
            var response = client.Post<AppRegistration>(request);

            return response.Data;
        }

        private Token GetTokenFromInstance(string instance, string code, string client_id, string client_secret, string redirect_uri)
        {
            var client = new RestClient(instance);

            var request = new RestRequest("oauth/token", DataFormat.Json)
                .AddJsonBody(new
                {
                    client_id,
                    client_secret,
                    redirect_uri,
                    code,
                    grant_type = "authorization_code",
                    scope = "read write follow push"
                });


            var response = client.Post<Token>(request);
            return response.Data;
        }

        private Account GetAccount(string token)
        {
            if (App == null)
            {
                throw new ArgumentException("Mastodon Application not set.");
            }

            var client = new RestClient(App.instance);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer");
            var request = new RestRequest("api/v1/accounts/verify_credentials");
            
            var response = client.Get<Account>(request);
            return response.Data;
        }

        public bool IsUrlForNetwork(Credentials credentials, string url)
        {
            // For now can only reply to toots on the same instance as the user
            // I have no idea if that is how it is supposed to work and I can
            // find nothing aboutt how one might arbitraily reply to a toot
            // elsewhere

            //https://mastodon.social/users/Gargron/statuses/1,
            //https://mastodon.social/@Gargron/1
            if (App == null)
            {
                SetInstance(credentials.Instance);
                if (App == null)
                {
                    throw new ArgumentException("Mastodon Application not set.");
                }
            }

            if (!url.StartsWith(App.instance))
            {
                return false;
            }

            return GetStatusIdFromTootUrl(url).HasValue;
        }

        private long? GetStatusIdFromTootUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var path = new Uri(url).AbsolutePath;

                var match = statusIdRegex.Match(path);

                if (match == null)
                {
                    match = statusIdRegexLong.Match(path);
                }

                if (match != null && match.Groups != null && match.Groups.Count > 0)
                {
                    if (long.TryParse(match.Groups[0].Value, out long id))
                    {
                        return id;
                    }
                }
            }

            return null;
        }


        private class AppRegistration
        {
            public string id { get; set; }
            public string name { get; set; }
            public string website { get; set; }
            public string redirect_uri { get; set; }
            public string client_id { get; set; }
            public string client_secret { get; set; }
            public string vapid_key { get; set; }

        }

        private class Token
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
            public string created_at { get; set; }
        }
    }

    public static class ServicesExtensions
    {
        public static void AddMastodon(this IServiceCollection services)
        {
            services.AddTransient<IMastodonAppStore, MastodonAppStore>();
            services.AddTransient<ISyndicationNetwork, Mastodon>();
        }
    }
}
