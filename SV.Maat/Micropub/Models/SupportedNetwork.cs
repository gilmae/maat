using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Micropub.Models
{
    public class SupportedNetwork
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        public NetworkDetails Network { get; set; }
    }

    public class NetworkDetails
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }
    }
}
