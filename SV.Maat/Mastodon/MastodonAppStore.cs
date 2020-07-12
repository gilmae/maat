using System;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using SV.Maat.lib.Repository;

namespace SV.Maat.Mastodon
{
    public class MastodonAppStore : RepositoryBase<MastodonApp>, IMastodonAppStore
    {
        public MastodonAppStore(IConfiguration config) : base(config.GetConnectionString("maat"))
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
