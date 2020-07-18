using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using SV.Maat.IndieAuth.Models;
using SimpleRepo;

namespace SV.Maat.IndieAuth
{
    public class AccessTokenStore : RepositoryBase<AccessToken>, IAccessTokenStore
    {
        public AccessTokenStore(IDbContext context) : base(context)
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
