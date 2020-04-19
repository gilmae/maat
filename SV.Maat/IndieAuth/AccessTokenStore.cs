using System;
using Microsoft.Extensions.Configuration;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth
{
    public class AccessTokenStore : RepositoryBase<AccessToken>
    {
        public AccessTokenStore(IConfiguration config) : base(config.GetConnectionString("Users"))
        {
        }
    }
}
