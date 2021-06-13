using System;
using System.Linq;
using System.Text.RegularExpressions;
using StrangeVanilla.Blogging.Events;


namespace SV.Maat.lib
{
    public static class EntryExtensions
    {
        public static PostType PostType(this Entry e)
        {
            // cf https://www.w3.org/TR/2018/NOTE-post-type-discovery-20180118/

            // event not supported
            // rsvp not supported
            // share not supported
            // favourite not supported
            // video not supported

            if (!string.IsNullOrEmpty(e.ReplyTo) && Uri.IsWellFormedUriString(e.ReplyTo, UriKind.Absolute))
            {
                return StrangeVanilla.Blogging.Events.PostType.reply;
            }

            if (!string.IsNullOrEmpty(e.BookmarkOf) && Uri.IsWellFormedUriString(e.ReplyTo, UriKind.Absolute))
            {
                return StrangeVanilla.Blogging.Events.PostType.bookmark;
            }

            if (e.AssociatedMedia != null && e.AssociatedMedia.Any())
            {
                return StrangeVanilla.Blogging.Events.PostType.photo;
            }

            Regex doubleWhiteSpace = new Regex(@"\s{2,}");
            string name = e.Title?.GetPlainText().Trim();

            if (string.IsNullOrEmpty(name))
            {
                return StrangeVanilla.Blogging.Events.PostType.note;
            }
            name = doubleWhiteSpace.Replace(name, " ");
            
            string content = e.Body?.GetPlainText()?.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                content = doubleWhiteSpace.Replace(content, " ");
            }            

            if (!(content??"").StartsWith(name))
            {
                return StrangeVanilla.Blogging.Events.PostType.article;
            }

            return StrangeVanilla.Blogging.Events.PostType.note;
        }
    }
}
