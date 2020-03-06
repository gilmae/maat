using System;
using System.IO;
using StrangeVanilla.Blogging.Events;

namespace StrangeVanilla.Maat.lib
{
    public static class UrlHelper
    {
        public static string EntryUrl(this Nancy.NancyContext ctx, Entry e)
        {
            var url = ctx.Request.Url;
            var path = Path.Join(url.SiteBase, "entries", e.Id.ToString());
            return path;
        }

        public static Guid GetEntryIdFromUrl(this Nancy.NancyContext ctx, string url)
        {
            Nancy.Url requestUrl = new Nancy.Url(url);
            var path = requestUrl.Path;

            if (path.StartsWith(@"/entries"))
            {
                var potentialId = path.Replace(@"/entries/", "");

                Guid entryId;

                if (Guid.TryParse(potentialId, out entryId))
                {
                    return entryId;
                }
            }

            return Guid.Empty;
        }

        public static string MediaUrl(this Nancy.NancyContext ctx,  Media e)
        {
            var url = ctx.Request.Url;
            var path = Path.Join(url.SiteBase, "media", e.Id.ToString());
            return path;
        }

        public static Guid GetMediaIdFromUrl(this Nancy.NancyContext ctx, string url)
        {
            Nancy.Url requestUrl = new Nancy.Url(url);
            var path = requestUrl.Path;

            if (path.StartsWith(@"/media"))
            {
                var potentialId = path.Replace(@"/media/", "");

                Guid mediaId;

                if (Guid.TryParse(potentialId, out mediaId))
                {
                    return mediaId;
                }
            }

            return Guid.Empty;
        }
    }
}
