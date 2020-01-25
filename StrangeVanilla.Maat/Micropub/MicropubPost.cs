using System;
using Newtonsoft.Json;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubPost
    {
        public string h { get; set; }
        public string content { get; set; }
        public string[] category { get; set; }
        
        public string name { get; set; }

        public string postStatus { get; set; }

        [JsonProperty("bookmark-of")]
        public string bookmark_of { get; set; }
    }
}
