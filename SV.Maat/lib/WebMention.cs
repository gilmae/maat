using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AngleSharp.Html.Parser;
using RestSharp;

namespace SV.Maat.lib
{
    public static class WebMention
    {
        public static (string link, string receiver) FindReceiver(this string link)
        {
            Uri linkUri = new Uri(link);
            RestClient client = new RestClient(linkUri.Host);
            client.FollowRedirects = true;

            var request = new RestRequest(linkUri.PathAndQuery);
            var response = client.Get(request);
            var headers = response.Headers.ToDictionary(h => h.Name, h => h.Value.ToString());
            var body = response.Content;

            return (link, FindReceiver(headers,body));
        }

        public static string FindReceiver(Dictionary<string, string> headers, string body)
        {
            var linkHeader = HttpHeaders.ParseHttpLinkHeader(headers["link"]).FirstOrDefault(h => h.Params.ContainsKey("rel") && h.Params["rel"] == "webmention");
            if (linkHeader != null)
            {
                return linkHeader.Url;
            }

            // Find first link in <head> with rel="webmention"
            var parser = new HtmlParser(new HtmlParserOptions
            {
                IsNotConsumingCharacterReferences = true,
            });
            var doc = parser.ParseDocument(body);
            var headLink = doc
                .QuerySelectorAll("head link[rel=webmention]")
                .Select(l => l.GetAttribute("href"))
                .FirstOrDefault(h => !string.IsNullOrEmpty(h));
            if (!string.IsNullOrEmpty(headLink))
            {
                return headLink;
            }

            // Find first <a rel="webmention">
            var bodyLink = doc
                .QuerySelectorAll("body a[rel=webmention]")
                .Select(l => l.GetAttribute("href"))
                .FirstOrDefault(h => !string.IsNullOrEmpty(h));
            if (!string.IsNullOrEmpty(bodyLink))
            {
                return bodyLink;
            }

            return string.Empty;
        }
    }
}
