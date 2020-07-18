using System;
using SimpleRepo;

namespace SV.Maat.Mastodon
{
    public class MastodonApp : Model
    {
        public string instance { get; set; }
        public string authenticationclient { get; set; }
    }
}
