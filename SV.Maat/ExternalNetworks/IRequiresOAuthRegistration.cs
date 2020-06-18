using System;

namespace SV.Maat.ExternalNetworks
{
    public interface IRequiresOAuthRegistration : ISyndicationNetwork
    {
        Uri GetAuthorizeLink(string redirectUri);
        Credentials GetToken(string redirectUri, string oauth_token, string oauth_verifier);
        string GetProfileUrl(Credentials credentials);
    }
}
