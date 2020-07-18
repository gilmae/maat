using Dapper;
using System.Collections.Generic;
using SimpleRepo;

namespace Users
{
    public class UserStore : RepositoryBase<User>, IUserStore
    {
        public UserStore(IDbContext context) : base(context)
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