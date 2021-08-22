using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using mf;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.Micropub.Models {
    [Vocab(MustBeType = new[] {"entry"})]
    public record Entry
    {
        [Id]
        public string Id { get; set; }

        [Property(Name="name")]
        [TypeConverter(typeof(MarkupConverter))]
        public MarkupObject Name { get; set; }

        [Property(Name = "summary")]
        [TypeConverter(typeof(MarkupConverter))]
        public MarkupObject Summary { get; set; }

        [Property(Name = "content")]
        [TypeConverter(typeof(MarkupConverter))]
        public MarkupObject Content { get; set; }

        [Property(Name = "published")]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime Published { get; set; }

        [Property(Name = "created")]
        public DateTime Created { get; set; }

        [Property(Name = "updated")]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime[] Updated { get; set; }

        [Property(Name = "author")]
        public object[] Author { get; set; }

        [Property(Name = "category")]
        public object[] Category { get; set; }

        [Property(Name = "url")]
        public object[] Url { get; set; }

        [Property(Name = "uid")]
        public string Uid { get; set; }

        [Property(Name = "location")]
        public object[] Location { get; set; }

        // Assume nobody is going to put a photo url here
        [Property(Name = "syndication")]
        public object[] Syndications { get; set; }

        [Property(Name = "in-reply-to")]
        public string ReplyTo { get; set; }

        [Property(Name = "rsvp")]
        public object[] Rsvp { get; set; }

        [Property(Name = "like-of")]
        public object[] LikeOf { get; set; }

        [Property(Name = "repost-of")]
        public object[] RepostOf { get; set; }

        [Property(Name = "bookmark-of")]
        public string BookmarkOf { get; set; }

        [Property(Name = "deleted")]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime Deleted { get; set; }

        [Property(Name="photo")]
        [TypeConverter(typeof(MediaConverter))]
        public MediaObject[] Photo { get; set; }

        public StatusType Status
        {
            get
            {
                if (Deleted <= DateTime.UtcNow)
                {
                    return StatusType.deleted;
                }
                else if (Published <= DateTime.UtcNow)
                {
                    return StatusType.published;
                }
                else
                {
                    return StatusType.draft;
                }
            }
        } 
    }

    public class MediaObject
    {
        public string Url { get; set; }
        public string Description { get; set; }
    }

public enum MarkupType {
    plain,
    html,
    markdown

}
    public class MarkupObject 
    {
        public string Value {get;set;}
        public string Markup {get;set;}
        public MarkupType Type {get;set;}
    }

    public class DateTimeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(object[]);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(DateTime[]);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var orig = value as object[];
            var dest = new DateTime[orig.Length];

            for (int i = 0;i<orig.Length;i++)
            {
                dest[i] = DateTime.Parse(orig[i].ToString());
            }
            return dest;
        }
    }

    public class MediaConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(object[]);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(DateTime[]);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var orig = value as object[];
            var dest = new MediaObject[orig.Length];

            for (int i = 0; i < orig.Length; i++)
            {
                dest[i] = new MediaObject();
                if (orig[i] is string)
                {
                    dest[i].Url = orig[i] as string;
                }
                else
                {
                    dest[i].Url = (orig[i] as Photo).Value;
                    dest[i].Description = (orig[i] as Photo).Alt;
                }
            }
            return dest;
        }
    }

        public class MarkupConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(object[]);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(MarkupObject[]);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var orig = value as object[];
            var dest = new MarkupObject[orig.Length];

            for (int i = 0; i < orig.Length; i++)
            {
                dest[i] = new MarkupObject();
                if (orig[i] is string)
                {
                    dest[i].Value = orig[i] as string;
                    dest[i].Markup = orig[i] as string;
                    dest[i].Type = MarkupType.plain;
                }
                else {
                    dynamic o = orig[i] as dynamic;
                    dest[i].Value = o.value;
                    if ((orig[i] as dynamic).html != null)
                    {
                        dest[i].Markup = o.html;
                        dest[i].Type = MarkupType.html;
                    }
                    else if ((orig[i] as dynamic).markdown != null)
                    {
                        dest[i].Markup = o.markdown;
                        dest[i].Type = MarkupType.markdown;
                    }
                }
            }
            return dest;
        }
    }
}