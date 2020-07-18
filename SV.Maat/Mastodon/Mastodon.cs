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

        public string Syndicate(Credentials credentials, Entry entry)
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

            var media_ids = entry.AssociatedMedia.Select(m =>
            {
                var media_request = new RestRequest("api/v2/media");
                byte[] data = Downloader.Download(m.Url).Result;
                media_request.AddFileBytes("file", data, Guid.NewGuid().ToString(), m.Type);
                media_request.AddParameter("description", m.Description);
                var response = client.Post<Attachment>(media_request);
                return response?.Data?.Id;
            }).ToList();

            var request = new RestRequest("api/v1/statuses")
                .AddJsonBody(new
                {
                    status = ContentHelper.GetPlainText(entry.Body),
                    media_ids
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
            string data = $"client_name={name}&redirect_uris={returnurls}&scopes=read+write+follow+push&website={url}";
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
