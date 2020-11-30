using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RestSharp;

namespace SV.Maat.lib
{
    public class HttpHeaders
    {
        const string paramsRegexPattern = "(\\w+)=['\"]?([^\"']+)['\"]?";
        const string urlRegex = "(?<=<).+?(?=>)";

        public static IEnumerable<HttpLink> ParseHttpLinkHeader(string link)
        {
            return link.Split(',').Select(l =>
                new HttpLink
                {
                    Url = Regex.Match(l, urlRegex, RegexOptions.IgnoreCase)?.Groups[0].Value,
                    Params = Regex.Matches(l, paramsRegexPattern, RegexOptions.IgnoreCase)
                        .SelectMany(m=>
                            m.Captures
                                .Where(c => (c as Match).Groups.Count == 3)
                        ).ToDictionary(
                            c => (c as Match).Groups[1].Value,
                            c => (c as Match).Groups[2].Value
                   )
                })
                .Where(l=>!string.IsNullOrEmpty(l.Url));
        }

        public static string GetContentType( string url)
        {
            Uri linkUri = new Uri(url);
            RestClient client = new RestClient(linkUri);
            client.FollowRedirects = true; var request = new RestRequest();
            var response = client.Head(request);
            return (string)response.Headers.SingleOrDefault(h => h.Name.ToLower() == "content-type")?.Value;


        }

        public class HttpLink
        {
            public string Url { get; set; }
            public Dictionary<string,string> Params { get; set; }
        }
    }
}
