using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Micropub.Models
{
    public class SyndicateToModel
    {
        [JsonPropertyName("syndicate-to")]
        public SupportedNetwork[] SyndicateTo { get; set; }
    }
}
