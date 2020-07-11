using System;
using StrangeVanilla.Blogging.Events;
using SV.Maat.ExternalNetworks;
using Mastonet;
using SV.Maat.IndieAuth;
using Mastonet.Entities;
using System.Threading.Tasks;

namespace SV.Maat.Mastodon
{
    public class Mastodon : IRequiresBearerToken, IRequiresFederatedInstance
    {
        public Mastodon()
        {
        }

        public string Name => "Mastodon";

        public string Photo => "";

        public string Url => "";

        public Application Application { get; set; }

        public string Syndicate(Credentials credentials, Entry entry)
        {
            throw new NotImplementedException();
        }
    }
}
