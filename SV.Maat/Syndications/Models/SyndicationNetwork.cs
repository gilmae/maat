using System;
using System.Collections.Generic;

namespace SV.Maat.Syndications.Models
{
    public class SyndicationNetwork
    {
        public string name { get; set; }
        public string url { get; set; }
        public string photo { get; set; }
        public string uidformat { get; set; }
    }

    public class SyndicationNetworks
    {
        public Dictionary<string, SyndicationNetwork> Networks { get; set; }
    }
}
