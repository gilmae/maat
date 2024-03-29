﻿using System.Text.Json.Serialization;
using SimpleDbRepository;


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
