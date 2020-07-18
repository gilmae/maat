using System;
using System.Collections.Generic;
using SimpleRepo;
using SV.Maat.Syndications.Models;

namespace SV.Maat.Syndications
{
    public interface ISyndicationStore : IRepository<Syndication>
    {
        IEnumerable<Syndication> FindByUser(int userid);
        Syndication FindByAccountName(string accountName);
    }
}
