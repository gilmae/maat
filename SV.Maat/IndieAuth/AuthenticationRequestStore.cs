using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth
{
    public class AuthenticationRequestStore : RepositoryBase<AuthenticationRequest>, IAuthenticationRequestStore
    {
        public AuthenticationRequestStore(IConfiguration config) : base(config.GetConnectionString("Users"))
        {
        }

        public AuthenticationRequest FindByAuthCode(string code)
        {
            using (Connection)
            {
                return Connection.Query<AuthenticationRequest>("select * from authenticationrequests where \"AuthorisationCode\" = @code", new { code }).FirstOrDefault();
            }
        }
    }
}
