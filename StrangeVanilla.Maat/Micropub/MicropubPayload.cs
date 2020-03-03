using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubPayload
    {
        public enum MicropubAction
        {
            update
        }

        [JsonProperty("type")]
        public string[] Type { get; set; }

        [JsonProperty("action")]
        public MicropubAction? Action { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string[]> Properties { get; set; }

        [JsonProperty("add")]
        public Dictionary<string, string[]> Add { get; set; }

        [JsonProperty("replace")]
        public Dictionary<string, string[]> Replace { get; set; }

        [JsonProperty("remove")]
        public string[] Remove { get; set; }

        public bool IsCreate()
        {
            return Properties?.Count > 0 && !Action.HasValue;
        }
    }
}
