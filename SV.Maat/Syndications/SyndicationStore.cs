using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Configuration;
using SV.Maat.lib.Repository;
using SV.Maat.Syndications.Models;

namespace SV.Maat.Syndications
{
    public class SyndicationStore : RepositoryBase<Syndication>, ISyndicationStore
    {
        public SyndicationStore(IConfiguration config) : base(config.GetConnectionString("Syndications"))
        {

        }

        public IEnumerable<Syndication> FindByUser(int userId) {
            using (Connection)
            {
                return Connection.Query<Syndication>("select * from Syndications where userid=@userID", new { userId });
            }
        }
    }
}
