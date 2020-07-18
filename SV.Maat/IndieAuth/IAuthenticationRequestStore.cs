using System;
using SV.Maat.IndieAuth.Models;
using SimpleRepo;

namespace SV.Maat.IndieAuth
{
    public interface IAuthenticationRequestStore : IRepository<AuthenticationRequest>
    {
        AuthenticationRequest FindByAuthCode(string code);
        AuthenticationRequest FindByAccessToken(string code);
    }
}
