using System;
using SV.Maat.lib.Repository;

namespace SV.Maat.Mastodon
{
    public interface IMastodonAppStore : IRepository<MastodonApp>
    {
        MastodonApp FindByInstance(string instance);
    }
}
