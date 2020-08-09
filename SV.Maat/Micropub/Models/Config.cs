using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SV.Maat.Micropub.Models
{
    public class Config
    {
        [JsonProperty("media-endpoint")]
        [JsonPropertyName("media-endpoint")]
        public string MediaEndpoint { get; set; }

        [JsonProperty("q")]
        [JsonPropertyName("media-endpoint")]
        public string[] SupportedQueries { get; set; }

        [JsonProperty("syndicate_to")]
        [JsonPropertyName("media-endpoint")]
        public object[] SupportedSyndicationNetworks { get; set; }

    }


}
