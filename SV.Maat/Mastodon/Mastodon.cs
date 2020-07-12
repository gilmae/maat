using System;
using StrangeVanilla.Blogging.Events;
using SV.Maat.ExternalNetworks;
using Mastonet;
using SV.Maat.IndieAuth;
using Mastonet.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

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

            return account["url"];
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
            throw new NotImplementedException();
        }

        private AppRegistration RegisterAppOnInstance(string instance, string name, string returnurls, string url)
        {
            string data = $"client_name={name}&redirect_uris={returnurls}&scopes=read+write+follow+push&website={url}";
            return System.Text.Json.JsonSerializer.Deserialize<AppRegistration>(MakePost($"{instance}/api/v1/apps", data));

        }

        private Token GetTokenFromInstance(string instance, string code, string client_id, string client_secret, string redirect_uri)
        {
            string data = $"client_id={client_id}&client_secret={client_secret}&redirect_uri={redirect_uri}&grant_type=authorization_code&code={code}&scope=read+write+follow+push";
            return System.Text.Json.JsonSerializer.Deserialize<Token>(MakePost($"{instance}/api/v1/apps", data));
        }

        private Dictionary<string,string> GetAccount(string token)
        {
            if (App == null)
            {
                throw new ArgumentException("Mastodon Application not set.");
            }

            string uri = $"{App.instance}/api/v1/accounts/verify_credentials";

            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string> > (MakeGet(uri, token));
        }

        private string MakePost(string uri, string data)
        {
            WebRequest r = WebRequest.Create(uri);
            r.Method = "POST";
            r.ContentType = "application/x-www-form-urlencoded";
            using (var s = r.GetRequestStream())
            {
                s.Write(Encoding.ASCII.GetBytes(data));
            }
            using (var resp = r.GetResponse())
            {
                using (var s = resp.GetResponseStream())
                {
                    using (var reader = new StreamReader(s))
                    {
                        var body = reader.ReadToEnd();
                        return body;
                    }
                }

            }
        }

        private string MakeGet(string uri, string token)
        {
            WebRequest r = WebRequest.Create(uri);
            if (!string.IsNullOrEmpty(token))
            {
                r.Headers.Add(HttpRequestHeader.Authorization, $"Bearer: {token}");
            }
            using (var resp = r.GetResponse())
            {
                using (var s = resp.GetResponseStream())
                {
                    using (var reader = new StreamReader(s))
                    {
                        var body = reader.ReadToEnd();
                        return body;
                    }
                }

            }
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
