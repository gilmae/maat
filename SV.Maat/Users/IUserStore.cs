using System;
using System.Collections.Generic;
using SV.Maat.lib.Repository;
using SV.Maat.Users.Models;

namespace SV.Maat.Users
{
    public interface IUserStore : IRepository<User>
    {
        public IEnumerable<User> FindByUsername(string username);
    }
}
