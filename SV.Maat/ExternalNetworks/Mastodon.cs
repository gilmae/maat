using System;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.ExternalNetworks
{
    public class Mastodon : IRequiresOAuthRegistration, IRequiresFederatedInstance

    {
        public Mastodon()
        {
        }

        public string Name => "Mastodon";

        public string Photo => "";

        public string Url => "";

        public Uri GetAuthorizeLink(string redirectUri)
        {
            throw new NotImplementedException();
        }

        public string GetProfileUrl(Credentials credentials)
        {
            throw new NotImplementedException();
        }

        public Credentials GetToken(string redirectUri, string oauth_token, string oauth_verifier)
        {
            throw new NotImplementedException();
        }

        public string Syndicate(Credentials credentials, Entry entry)
        {
            throw new NotImplementedException();
        }
    }
}
