using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public partial class Account
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("acct")]
        public string Acct { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("locked")]
        public bool Locked { get; set; }

        [JsonPropertyName("bot")]
        public bool Bot { get; set; }

        [JsonPropertyName("discoverable")]
        public bool Discoverable { get; set; }

        [JsonPropertyName("group")]
        public bool Group { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("avatar")]
        public Uri Avatar { get; set; }

        [JsonPropertyName("avatar_static")]
        public Uri AvatarStatic { get; set; }

        [JsonPropertyName("header")]
        public Uri Header { get; set; }

        [JsonPropertyName("header_static")]
        public Uri HeaderStatic { get; set; }

        [JsonPropertyName("followers_count")]
        public long FollowersCount { get; set; }

        [JsonPropertyName("following_count")]
        public long FollowingCount { get; set; }

        [JsonPropertyName("statuses_count")]
        public long StatusesCount { get; set; }

        [JsonPropertyName("last_status_at")]
        public DateTimeOffset LastStatusAt { get; set; }

        [JsonPropertyName("emojis")]
        public object[] Emojis { get; set; }

        [JsonPropertyName("fields")]
        public Field[] Fields { get; set; }
    }
}
