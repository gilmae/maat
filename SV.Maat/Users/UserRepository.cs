using System;
using SV.Maat.lib.Repository;
using SV.Maat.Users.Models;
using Dapper.Contrib.Extensions;
using Dapper;
using System.Collections.Generic;

namespace SV.Maat.Users
{
    public class UserStore : RepositoryBase<User>, IUserStore
    {
        public IEnumerable<User> FindByUsername(string username)
        {
            return base._connection.Query<User>("select * from User where username=@username", new { username });
        }
    }
}
