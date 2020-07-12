using System;
using Microsoft.AspNetCore.Http;

namespace SV.Maat.ExternalNetworks
{
    public interface IRequiresOAuthRegistration : ISyndicationNetwork
    {
        Uri GetAuthorizeLink(string redirectUri);
        Credentials GetToken(string redirectUri, IQueryCollection query);
        string GetProfileUrl(Credentials credentials);
    }
}
