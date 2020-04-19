using System;
using System.Text.Json.Serialization;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth.Models
{
    public class AccessToken : Model
    {
        [JsonIgnore]
        public int AuthenticationRequestId { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
