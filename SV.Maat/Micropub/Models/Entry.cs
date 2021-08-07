using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using mf;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.Micropub.Models {
    [Vocab(MustBeType = new[] {"h-entry"})]
    public record Entry
    {
        [Id]
        public string Id { get; set; }

        [Property(Name="name")]
        public object[] Name { get; set; }

        [Property(Name = "summary")]
        public string[] Summary { get; set; }

        [Property(Name = "content")]
        public string Content { get; set; }

        [Property(Name = "published")]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime Published { get; set; }

        [Property(Name = "updated")]
        public object[] Updated { get; set; }

        [Property(Name = "author")]
        public object[] Author { get; set; }

        [Property(Name = "category")]
        public object[] Category { get; set; }

        [Property(Name = "url")]
        public object[] Url { get; set; }

        [Property(Name = "uid")]
        public object[] Uid { get; set; }

        [Property(Name = "location")]
        public object[] Location { get; set; }

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
        public object[] BookmarkOf { get; set; }

        [Property(Name = "deleted")]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime Deleted { get; set; }

        [Property(Name = "slug")]
        public string[] Slug { get; set; }

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
}