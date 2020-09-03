using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using HeyRed.MarkdownSharp;
using Newtonsoft.Json.Linq;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.lib
{
    public static class ContentHelper
    {
        private const string uriPattern = @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)";


        public static IEnumerable<string> DiscoverLinks(this Content content)
        {
            return content.Markup.DiscoverLinks().Union(content.Value.DiscoverLinks()).Distinct();
        }

        public static IEnumerable<string> DiscoverLinks(this string content)
        {
            return Regex.Matches(content, uriPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
                .SelectMany(m => m.Captures.Select(c => c.Value))
                .Distinct();
        }

        public static string GetHtml(this Content content)
        {
            if (content.Type == ContentType.plaintext)
            {
                return content.Value;
            }
            else if (content.Type == ContentType.html)
            {
                return content.Markup;
            }
            else if (content.Type == ContentType.markdown)
            {
                return new Markdown().Transform(content.Markup);
            }

            return string.Empty;
        }

        public static string GetPlainText(this Content content)
        {
            return content?.Value;
        }

        public static string GetPlainText(this string[] content)
        {
            if (content == null || content.IsEmpty())
            {
                return "";
            }
            return content[0];
        }

        public static Content ParseContentArray(this object[] values)
        {
            if (values is null)
            {
                return null;
            }
            List<KeyValuePair<ContentType, string>> contents = new List<KeyValuePair<ContentType, string>>();

           foreach (object v in values)
            {
                if (v is string && !string.IsNullOrEmpty((string)v))
                {
                    return new Content { Type = ContentType.plaintext, Value = (string)v };
                }
                else if (v is JObject)
                {
                    dynamic dict = v as JObject;

                    if (string.IsNullOrEmpty((string)dict.value)
                        && string.IsNullOrEmpty((string)dict.html)
                        && string.IsNullOrEmpty((string)dict.markdown))
                    {
                        continue;
                    }

                    if (dict.html != null)
                    {
                        return new Content { Type = ContentType.html, Value = (string)dict.value, Markup=(string)dict.html };
                    }
                    if (dict.markdown != null)
                    {
                        return new Content { Type = ContentType.markdown, Value = (string)dict.value, Markup = (string)dict.markdown };
                    }
                }
            }

            return null;
        }
    }
}
