using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubPayload
    {
        [JsonProperty("type")]
        public string[] Type { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string[]> Properties { get; set; }

        [JsonProperty("add")]
        public MicropubPost Add { get; set; }

        [JsonProperty("replace")]
        public MicropubPost Replace { get; set; }

        [JsonProperty("remove")]
        public string[] Remove { get; set; }
    }
}
