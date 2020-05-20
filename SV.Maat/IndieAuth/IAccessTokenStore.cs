using System;
using System.Collections.Generic;
using SV.Maat.IndieAuth.Models;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth
{
    public interface IAccessTokenStore : IRepository<AccessToken>
    {
        IList<AccessToken> FindByUser(int userId);
    }
}
