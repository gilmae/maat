using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Micropub.Models
{
    public class Config
    {
        [JsonPropertyName("media-endpoint")]
        public string MediaEndpoint { get; set; }

    }


}
