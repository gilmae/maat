using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth
{
    public class AccessTokenStore : RepositoryBase<AccessToken>, IAccessTokenStore
    {
        public AccessTokenStore(IConfiguration config) : base(config.GetConnectionString("maat"))
        {
        }

        public IList<AccessToken> FindByUser(int userId)
        {
            using (Connection)
            {
                return Connection.Query<AccessToken>("select * from AccessTokens where \"UserId\" = @userId", new { userId }).ToList();
            }
        }
    }
}
