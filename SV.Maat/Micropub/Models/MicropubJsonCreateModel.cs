using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SV.Maat.Micropub.Models
{
    public class MicropubJsonCreateModel
    {
        [JsonPropertyName("type")]
        public string[] Type { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, dynamic[]> Properties { get; set; }
    }
}
