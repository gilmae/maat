using System;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth
{
    public interface IAuthenticationRequestStore : IRepository<AuthenticationRequest>
    {
        AuthenticationRequest FindByAuthCode(string code);
        AuthenticationRequest FindByAccessToken(string code);
    }
}
