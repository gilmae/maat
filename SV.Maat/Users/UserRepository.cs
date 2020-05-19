using System;
using SV.Maat.lib.Repository;
using SV.Maat.Users.Models;
using Dapper.Contrib.Extensions;
using Dapper;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SV.Maat.Users
{
    public class UserStore : RepositoryBase<User>, IUserStore
    {
        public UserStore(IConfiguration config) :base(config.GetConnectionString("Users"))
        {
            
        }

        public IEnumerable<User> FindByUsername(string username)
        {
            using (Connection)
            {
                return Connection.Query<User>("select * from users where \"Username\"=@username", new { username });
            }
        }
    }
}
