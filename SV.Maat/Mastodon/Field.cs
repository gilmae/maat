using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public partial class Field
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("verified_at")]
        public DateTimeOffset? VerifiedAt { get; set; }
    }
}
