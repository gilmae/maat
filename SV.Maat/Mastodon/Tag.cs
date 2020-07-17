using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public class Tag
    {
        public Tag() { }
        /// <summary>
        /// The hashtag, not including the preceding #
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the hashtag
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 7-day stats of the hashtag
        /// </summary>
        [JsonPropertyName("history")]
        public IEnumerable<History>? History { get; set; }
    }

    public class History
    {
        /// <summary>
        /// UNIX time of the beginning of the day
        /// </summary>
        [JsonPropertyName("day")]
        public int Day { get; set; }

        /// <summary>
        /// Number of statuses with the hashtag during the day
        /// </summary>
        [JsonPropertyName("uses")]
        public int Uses { get; set; }

        /// <summary>
        /// Number of accounts that used the hashtag during the day
        /// </summary>
        [JsonPropertyName("accounts")]
        public int Accounts { get; set; }
    }
}
