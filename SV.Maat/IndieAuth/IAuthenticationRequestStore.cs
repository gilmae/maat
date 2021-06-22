using System;
using SV.Maat.IndieAuth.Models;
using SimpleDbContext;
using SimpleDbRepository;


namespace SV.Maat.IndieAuth
{
    public interface IAuthenticationRequestStore : IRepository<AuthenticationRequest>
    {
        AuthenticationRequest FindByAuthCode(string code);
        AuthenticationRequest FindByAccessToken(string code);
    }
}
