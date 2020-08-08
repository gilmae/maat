using StrangeVanilla.Blogging.Events;
using RestSharp;
using SV.Maat.lib;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;

namespace SV.Maat.ExternalNetworks
{
    public class Pinboard : ISyndicationNetwork, IRequiresCredentialEntry
    {
        public Pinboard()
        {
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

        public string Syndicate(Credentials credentials, Entry entry, string inNetworkReplyingTo = null)
        {
            RestClient client = new RestClient("https://api.pinboard.in");
            var request = new RestRequest("v1/posts/add")
                .AddQueryParameter("url", entry.BookmarkOf)
                .AddQueryParameter("description", ContentHelper.GetPlainText(entry.Title))
                .AddQueryParameter("extended", ContentHelper.GetPlainText(entry.Body))
                .AddQueryParameter("tags", string.Join(',', entry.Categories))
                .AddQueryParameter("format", "json")
                .AddQueryParameter("auth_token", $"{credentials.Uid}:{credentials.Secret}");

            var result = client.Get<BookmarkPostResult>(request);

            if (result.Data.ResultCode == "done")
            {
                return $"pinboard:{System.Web.HttpUtility.UrlEncode(entry.BookmarkOf)}";
            }
            return "";
        }

        public struct BookmarkPostResult
        {
            [JsonPropertyName("result_code")]
            public string ResultCode { get; set; }
        }
    }
}
