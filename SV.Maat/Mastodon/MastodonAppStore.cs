using System.Linq;
using Dapper;
using SimpleDbContext;
using SimpleDbRepository;

namespace SV.Maat.Mastodon
{
    public class MastodonAppStore : RepositoryBase<MastodonApp>, IMastodonAppStore
    {
        public MastodonAppStore(IDbContext context) : base(context)
        {

        }

        public MastodonApp FindByInstance(string instance)
        {
            using (Connection)
            {
                return Connection.Query<MastodonApp>("select * from mastodonapps where instance=@instance", new { instance }).LastOrDefault();
            }
        }
    }
}
