using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SV.Maat.Micropub.Models
{
    public class SyndicateToModel
    {
        [JsonPropertyName("syndicate-to")]
        [JsonProperty("syndicate-to")]
        public SupportedNetwork[] SyndicateTo { get; set; }
    }
}
