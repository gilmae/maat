using System.Collections.Generic;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.ExternalNetworks
{
    public interface ISyndicationNetwork
    {
        string Name { get; }
        string Photo { get; }
        string Url { get; }

        string Syndicate(Credentials credentials, Entry entry, string inNetworkReplyingTo = null);

        bool IsUrlForNetwork(string url);
    }
}
