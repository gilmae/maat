using System.Text.Json.Serialization;

namespace SV.Maat.Mastodon
{
    public partial class Application
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("website")]
        public object Website { get; set; }
    }
}
