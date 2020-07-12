using System;
using SV.Maat.lib.Repository;

namespace SV.Maat.Mastodon
{
    public class MastodonApp : Model
    {
        public string instance { get; set; }
        public string authenticationclient { get; set; }
    }
}
