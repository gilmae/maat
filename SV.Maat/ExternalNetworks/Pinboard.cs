using StrangeVanilla.Blogging.Events;
using RestSharp;
using SV.Maat.lib;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace SV.Maat.ExternalNetworks
{
    public class Pinboard : ISyndicationNetwork, IRequiresCredentialEntry
    {

        ILogger<Pinboard> _logger;

        public Pinboard(ILogger<Pinboard> logger)
        {
            _logger = logger;
        }

        public string Name => "Pinboard";

        public string Photo => "https://pinboard.in/bluepin.gif";

        public string Url => "https://pinboard.in";

        public bool IsUrlForNetwork(Credentials credentials, string url)
        {
            if (url.StartsWith("pinboard:"))
            {
                string bookmark = url.Remove(0, "pinboard:".Length);

                return Uri.IsWellFormedUriString(System.Web.HttpUtility.UrlDecode(bookmark), UriKind.RelativeOrAbsolute);
            }
            return false;
        }

        public string Syndicate(Credentials credentials, Post post, string inNetworkReplyingTo = null)
        {
            Micropub.Models.Entry entry = Post.AsVocab<Micropub.Models.Entry>();
            if (entry.IsNull())
            {
               return "";
            }
            RestClient client = new RestClient("https://api.pinboard.in");
            var request = new RestRequest("v1/posts/add")
               .AddQueryParameter("url", entry.BookmarkOf)
               .AddQueryParameter("description", entry.Name.Value)
               .AddQueryParameter("extended", entry.Content.Value)
               .AddQueryParameter("tags", string.Join(',', entry.Category))
               .AddQueryParameter("format", "json")
               .AddQueryParameter("auth_token", $"{credentials.Uid}:{credentials.Secret}");

            _logger.LogDebug($"Pinboard Request: {request}");

            var result = client.Get<BookmarkPostResult>(request);

            _logger.LogDebug($"Pinboard Response: {result}");

            if (result.Data.ResultCode == "done")
            {
               return $"pinboard:{System.Web.HttpUtility.UrlEncode(entry.BookmarkOf)}";
            }

            _logger.LogError($"Syndicating link to Pinboard for Entry {entry.Id} failed.");
            _logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(new
            {
               result.StatusCode,
               result.StatusDescription,
               result.Data
            }));
            return "";
        }

        public struct BookmarkPostResult
        {
            [JsonPropertyName("result_code")]
            public string ResultCode { get; set; }
        }
    }
}
