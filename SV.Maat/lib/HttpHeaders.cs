using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
                    Url = Regex.Match(l, urlRegex)?.Groups[0].Value,
                    Params = Regex.Matches(l, paramsRegexPattern)
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

        public class HttpLink
        {
            public string Url { get; set; }
            public Dictionary<string,string> Params { get; set; }
        }
    }
}
