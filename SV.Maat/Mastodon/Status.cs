using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public partial class Status
    {
        public Status() { }
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("in_reply_to_id")]
        public long? InReplyToId { get; set; }

        [JsonPropertyName("in_reply_to_account_id")]
        public long? InReplyToAccountId { get; set; }

        [JsonPropertyName("sensitive")]
        public bool Sensitive { get; set; }

        [JsonPropertyName("spoiler_text")]
        public string SpoilerText { get; set; }

        [JsonPropertyName("visibility")]
        public string Visibility { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("uri")]
        public Uri Uri { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("replies_count")]
        public long RepliesCount { get; set; }

        [JsonPropertyName("reblogs_count")]
        public long ReblogsCount { get; set; }

        [JsonPropertyName("favourites_count")]
        public long FavouritesCount { get; set; }

        [JsonPropertyName("favourited")]
        public bool Favourited { get; set; }

        [JsonPropertyName("reblogged")]
        public bool Reblogged { get; set; }

        [JsonPropertyName("muted")]
        public bool Muted { get; set; }

        [JsonPropertyName("bookmarked")]
        public bool Bookmarked { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("reblog")]
        public Status Reblog { get; set; }

        [JsonPropertyName("application")]
        public Application Application { get; set; }

        [JsonPropertyName("account")]
        public Account Account { get; set; }

        [JsonPropertyName("media_attachments")]
        public IEnumerable<Attachment> MediaAttachments { get; set; }

        [JsonPropertyName("mentions")]
        public IEnumerable<Mention> Mentions { get; set; }

        [JsonPropertyName("tags")]
        public IEnumerable<Tag> Tags { get; set; }

        [JsonPropertyName("emojis")]
        public IEnumerable<Emoji> Emojis { get; set; }

        [JsonPropertyName("card")]
        public Card Card { get; set; }

        [JsonPropertyName("poll")]
        public Poll Poll { get; set; }
    }

   
}
