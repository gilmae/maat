using System;
using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public partial class Emoji
    {
        public Emoji() { }

        /// <summary>
        /// The shortcode of the emoji
        /// </summary>
        [JsonPropertyName("shortcode")]
        public string Shortcode { get; set; } = string.Empty;

        /// <summary>
        /// URL to the emoji static image
        /// </summary>
        [JsonPropertyName("static_url")]
        public string StaticUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL to the emoji image
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Boolean that indicates if the emoji is visible in picker
        /// </summary>
        [JsonPropertyName("visible_in_picker")]
        public bool VisibleInPicker { get; set; }
    }
}
