using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.lib
{
    public class ContentHelper
    {

        public static string GetPlainText(KeyValuePair<ContentType, string>[] content)
        {
            if (content == null || content.IsEmpty())
            {
                return "";
            }
            if (content.Any(c => c.Key == ContentType.plaintext))
            {
                return content.FirstOrDefault(c => c.Key == ContentType.plaintext).Value;
            }

            if (content.Any(c => c.Key == ContentType.html))
            {
                // TODO Parse and extract TextNodes
                return content.FirstOrDefault(c => c.Key == ContentType.html).Value; 
            }

            if (content.Any(c => c.Key == ContentType.markdown))
            {
                return content.FirstOrDefault(c => c.Key == ContentType.markdown).Value;
            }

            return "";

        }

        public static string GetPlainText(string[] content)
        {
            if (content == null || content.IsEmpty())
            {
                return "";
            }
            return content[0];
        }

        public static KeyValuePair<ContentType, string>[] ParseContentArray(object[] values)
        {
            if (values is null)
            {
                return null;
            }
            List<KeyValuePair<ContentType, string>> contents = new List<KeyValuePair<ContentType, string>>();
            foreach (object v in values)
            {
                if (v is string)
                {
                    contents.Add(new KeyValuePair<ContentType, string>(ContentType.plaintext, (string)v));
                }
                else if (v is JObject)
                {

                    dynamic dict = v as JObject;
                    if (dict.html != null)
                    {
                        contents.Add(new KeyValuePair<ContentType, string>(ContentType.html, (string)dict.html));
                    }
                    if (dict.markdown != null)
                    {
                        contents.Add(new KeyValuePair<ContentType, string>(ContentType.markdown, (string)dict.markdown));
                    }
                }
            }

            return contents.ToArray();
        }
    }
}
