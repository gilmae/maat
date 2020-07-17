using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public partial class Application
    {
        public Application() { }
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("website")]
        public string Website { get; set; }
    }
}
