using System.Collections.Generic;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.ExternalNetworks
{
    public interface ISyndicationNetwork
    {
        string Name { get; }
        string Photo { get; }
        string Url { get; }

        string Syndicate(Credentials credentials, Post entry, string inNetworkReplyingTo = null);

        bool IsUrlForNetwork(Credentials credentials, string url);
    }
}
