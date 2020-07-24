using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SV.Maat.IndieAuth.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("scope")]
        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("me")]
        [JsonProperty("me")]
        public string UserProfileUrl { get; set; }

        [JsonPropertyName("access_token")]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
