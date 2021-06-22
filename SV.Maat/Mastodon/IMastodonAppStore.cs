using SimpleDbRepository;

namespace SV.Maat.Mastodon
{
    public interface IMastodonAppStore : IRepository<MastodonApp>
    {
        MastodonApp FindByInstance(string instance);
    }
}
