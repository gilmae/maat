using System.Collections.Generic;
using System.Linq;
using Dapper;
using SimpleDbContext;
using SimpleDbRepository;
using SV.Maat.Syndications.Models;

namespace SV.Maat.Syndications
{
    public class SyndicationStore : RepositoryBase<Syndication>, ISyndicationStore
    {
        public SyndicationStore(IDbContext context) : base(context)
        {

        }

        public IEnumerable<Syndication> FindByUser(int userId) {
            using (Connection)
            {
                return Connection.Query<Syndication>("select * from Syndications where \"UserId\"=@userId", new { userId });
            }
        }

        public Syndication FindByAccountName(string accountName)
        {
            using (Connection)
            {
                return Connection.Query<Syndication>("select * from Syndications where \"AccountName\"=@accountName", new { accountName }).FirstOrDefault();
            }
        }
    }
}
