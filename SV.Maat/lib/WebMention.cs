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
            RestClient client = new RestClient(linkUri);
            client.FollowRedirects = true;

            var request = new RestRequest();
            var response = client.Get(request);
            var headers = response.Headers.ToDictionary(h => h.Name.ToLower(), h => h.Value.ToString());
            var body = response.Content;

            string receiver = FindReceiver(headers, body);

            if (string.IsNullOrEmpty(receiver))
            {
                return (link, string.Empty);
            }

            if (!Uri.IsWellFormedUriString(receiver, UriKind.Absolute))
            {
                receiver = new Uri(linkUri, receiver).AbsoluteUri;
            }

            if (Uri.IsWellFormedUriString(receiver, UriKind.Relative))
            {
                receiver = Path.Join(link, receiver);
            }

            return (link, receiver);
        }

        public static string FindReceiver(Dictionary<string, string> headers, string body)
        {
            if (headers.ContainsKey("link"))
            {
                var linkHeader = HttpHeaders.ParseHttpLinkHeader(headers["link"]).FirstOrDefault(h => h.Params.ContainsKey("rel") && h.Params["rel"].ToLower().Split(' ').Contains("webmention"));
                if (linkHeader != null)
                {
                    return linkHeader.Url;
                }
            }

            // Find first link in <head> with rel="webmention"
            var parser = new HtmlParser(new HtmlParserOptions
            {
                IsNotConsumingCharacterReferences = true,
            });
            var doc = parser.ParseDocument(body);
            var htmlLink = doc
                .QuerySelectorAll("link[rel~=webmention], body a[rel~=webmention]")
                .Select(l => l.GetAttribute("href"))
                .FirstOrDefault(h => !string.IsNullOrEmpty(h));
            if (!string.IsNullOrEmpty(htmlLink))
            {
                return htmlLink;
            }


            return string.Empty;
        }

        public static bool SendWebMention(string source, string target, string receiver)
        {
            var client = new RestClient(receiver);
            var request = new RestRequest()
                .AddParameter("source", source)
                .AddParameter("target", target);

            var response = client.Post(request);

            return response.IsSuccessful;
            
        }
    }
}
