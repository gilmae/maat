using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using SV.Maat.IndieAuth.Models;
using SimpleDbContext;
using SimpleDbRepository;


namespace SV.Maat.IndieAuth
{
    public class AuthenticationRequestStore : RepositoryBase<AuthenticationRequest>, IAuthenticationRequestStore
    {
        public AuthenticationRequestStore(IDbContext context) : base(context)
        {
        }

        public AuthenticationRequest FindByAuthCode(string code)
        {
            using (Connection)
            {
                return Connection.Query<AuthenticationRequest>("select * from authenticationrequests where \"AuthorisationCode\" = @code", new { code }).FirstOrDefault();
            }
        }


        public AuthenticationRequest FindByAccessToken(string hashedToken)
        {
            using (Connection)
            {
                return Connection.Query<AuthenticationRequest>("select * from authenticationrequests where \"AccessCode\" = @code", new { code=hashedToken }).FirstOrDefault();
            }
        }
    }
}
