using System;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using SimpleRepo;

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

        [JsonIgnore]
        public string Scope { get; set; }

        [JsonIgnore]
        public string ClientId { get; set; }

    }
}
