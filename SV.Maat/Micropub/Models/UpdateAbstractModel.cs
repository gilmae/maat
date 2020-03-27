using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SV.Maat.Micropub.Models
{
    public class UpdateModel
    {
        public string Action { get; set; }
        public string Url { get; set; }

        [JsonPropertyName("add")]
        public Dictionary<string, string[]> Add { get; set; }

        [JsonPropertyName("remove")]
        public string[] Remove { get; set; }

        [JsonPropertyName("replace")]
        public Dictionary<string, string[]> Replace { get; set; }
    }
}
