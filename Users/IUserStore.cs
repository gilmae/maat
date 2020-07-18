using System;
using System.Collections.Generic;
using SimpleRepo;


namespace Users
{
    public interface IUserStore : IRepository<User>
    {
        public IEnumerable<User> FindByUsername(string username);
    }
}
