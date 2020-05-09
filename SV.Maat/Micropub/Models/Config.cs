using Newtonsoft.Json;

namespace SV.Maat.Micropub.Models
{
    public class Config
    {
        [JsonProperty("media-endpoint")]
        public string MediaEndpoint { get; set; }

        [JsonProperty("q")]
        public string[] SupportedQueries { get; set; }

        [JsonProperty("syndicate_to")]
        public object[] SupportedSyndicationNetworks { get; set; }

    }


}
