using Newtonsoft.Json;

namespace StrangeVanilla.Maat.Micropub
{
    public class Config
    {
        [JsonProperty("media-endpoint")]
        public string MediaEndpoint { get; set; }
    }
}
