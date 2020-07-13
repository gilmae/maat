using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public partial class Status
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("in_reply_to_id")]
        public object InReplyToId { get; set; }

        [JsonPropertyName("in_reply_to_account_id")]
        public object InReplyToAccountId { get; set; }

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
        public object Reblog { get; set; }

        [JsonPropertyName("application")]
        public Application Application { get; set; }

        [JsonPropertyName("account")]
        public Account Account { get; set; }

        [JsonPropertyName("media_attachments")]
        public object[] MediaAttachments { get; set; }

        [JsonPropertyName("mentions")]
        public object[] Mentions { get; set; }

        [JsonPropertyName("tags")]
        public object[] Tags { get; set; }

        [JsonPropertyName("emojis")]
        public object[] Emojis { get; set; }

        [JsonPropertyName("card")]
        public Card Card { get; set; }

        [JsonPropertyName("poll")]
        public object Poll { get; set; }
    }

    public partial class Application
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("website")]
        public object Website { get; set; }
    }

    public partial class Card
    {
        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("author_name")]
        public string AuthorName { get; set; }

        [JsonPropertyName("author_url")]
        public string AuthorUrl { get; set; }

        [JsonPropertyName("provider_name")]
        public string ProviderName { get; set; }

        [JsonPropertyName("provider_url")]
        public string ProviderUrl { get; set; }

        [JsonPropertyName("html")]
        public string Html { get; set; }

        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("image")]
        public object Image { get; set; }

        [JsonPropertyName("embed_url")]
        public string EmbedUrl { get; set; }
    }
}
