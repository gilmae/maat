using System;
using Microsoft.Extensions.Configuration;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth
{
    public class AuthenticationRequestionStore : RepositoryBase<AuthenticationRequest>
    {
        public AuthenticationRequestionStore(IConfiguration config) : base(config.GetConnectionString("Users"))
        {
        }
    }
}
