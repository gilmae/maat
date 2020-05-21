using System;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using SV.Maat.lib.Repository;

namespace SV.Maat.IndieAuth.Models
{
    public class AccessToken : Model
    {
        [JsonIgnore]
        public int AuthenticationRequestId { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public string Scope { get; set; }

        [JsonIgnore]
        public string ClientId { get; set; }

        public string Salt { get; set; }

        public AccessToken()
        {
            Salt = Guid.NewGuid().ToString();
        }

        public string Token()
        {
            return Convert.ToBase64String(
                Encoding.ASCII.GetBytes(
                    JsonSerializer.Serialize(this)));
        }
    }
}
