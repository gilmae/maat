using Newtonsoft.Json;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubPost
    {
        [JsonProperty("h")]
        public string Type { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("name")]
        public string Title { get; set; }

        [JsonProperty("category")]
        public string[] Categories { get; set; }

        [JsonProperty("post-status")]
        public string PostStatus { get; set; }

        [JsonProperty("bookmark-of")]
        public string BookmarkOf { get; set; }
    }
}
