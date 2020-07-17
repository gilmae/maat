using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public class Mention
    {
        public Mention() { }
        /// <summary>
        /// Account ID
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// URL of user's profile (can be remote)
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The username of the account
        /// </summary>
        [JsonPropertyName("username")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Equals username for local users, includes @domain for remote ones
        /// </summary>
        [JsonPropertyName("acct")]
        public string AccountName { get; set; } = string.Empty;

    }
}
