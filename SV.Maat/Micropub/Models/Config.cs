using Newtonsoft.Json;

namespace SV.Maat.Micropub.Models
{
    public class Config
    {
        [JsonProperty("media-endpoint")]
        public string MediaEndpoint { get; set; }

    }


}
