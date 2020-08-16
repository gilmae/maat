using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StrangeVanilla.Blogging.Events;
using SV.Maat.lib;
using static StrangeVanilla.Blogging.Events.Entry;

namespace SV.Maat.Micropub
{
    public class EntryToMicropubConverter
    {
        private Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();
        private static Dictionary<string, string> columnMapper = new Dictionary<string, string>
            {
                {"name", "Title"},
                {"content", "Body" },
                {"category", "Categories" },
                {"photo", "AssociatedMedia" },
                {"in-reply-to", "ReplyTo" },
                {"post-status", "Status" },
                {"published", "PublishedAt" },
                {"bookmark-of", "BookmarkOf" },
                { "dt-deleted", "DeletedAt"},
                {"mp-syndicate-to", "SyndicateTo" },
                {"syndication", "Syndications" }
            };

        public EntryToMicropubConverter() : this(null) { }
        public EntryToMicropubConverter(IList<string> columnsRequired)
        {
            if (columnsRequired == null || columnsRequired.Count == 0)
            {
                columnsRequired = columnMapper.Keys.ToList();
            }

            columnsRequired = columnsRequired.Where(c => columnMapper.ContainsKey(c)).ToList();

            Type entry = typeof(Entry);
            foreach (string column in columnsRequired)
            {
                var property = entry.GetProperty(columnMapper[column]);
                if (property != null)
                {
                    _properties[column] = property;
                }
            }
        }

        public dynamic GetMicropub(Entry entry, Func<Entry, string> urlGenerator, bool forceIncludeType = false)
        {
            Dictionary<string, object> enrichedItem = new Dictionary<string, object>();
            var properties = ToDictionary(entry);
            enrichedItem["properties"] = properties;

            if (urlGenerator != null)
            {
                enrichedItem["url"] = new[] { urlGenerator.Invoke(entry) };
            }
            if (properties.IsEmpty() || forceIncludeType)
            {
                enrichedItem["type"] = "h-entry";
            }
            return enrichedItem;
        }

        public Dictionary<string, object> ToDictionary(Entry entry)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            foreach (string key in _properties.Keys)
            {
                values[key] = GetValue(key, entry);
            }

            return values;

        }

        public object GetValue(string columnName, Entry entry)
        {
            object value = _properties[columnName].GetValue(entry);
            if (value == null)
            {
                return null;
            }

            switch (columnName)
            {
                case "in-reply-to":
                case "bookmark-of":
                case "category":
                case "published":
                case "dt-deleted":
                case "mp-syndicate-to":
                case "syndication":
                    return value;
                case "photo":
                    var media = value as IList<MediaLink>;
                    if (media != null)
                    {
                        return media.Where(x => x.Type == "photo");
                    }
                    return null;
                case "post-status":
                    return value.ToString();
                case "content":
                case "name":
                    var contents = new List<object>();
                    var content = (Content)value;

                    if (content == null)
                    {
                        return contents;
                    }

                    if (content.Type == ContentType.plaintext)
                    {
                        contents.Add(content.Value);
                    }
                    else if (content.Type == ContentType.html)
                    {
                        contents.Add(new { html = content.Markup, value = content.Value });
                    }
                    else if (content.Type == ContentType.markdown)
                    {
                        contents.Add(new { markdown = content.Markup, value = content.Value });
                    }

                    return contents;
                default:
                    return null;

            }
        }
    }
}
