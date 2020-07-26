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

        public static string GetPlainText(Content content)
        {
            return content?.Value;
        }

        public static string GetPlainText(string[] content)
        {
            if (content == null || content.IsEmpty())
            {
                return "";
            }
            return content[0];
        }

        public static Content ParseContentArray(object[] values)
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
