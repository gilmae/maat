using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.lib
{
    public static class UrlHelper
    {
        public static string EntryUrl(this HttpContext ctx, Entry e)
        {
             var path = ctx.Request.Scheme + "://" + Path.Join(ctx.Request.Host.ToString(), ctx.Request.PathBase, "entries", e.Id.ToString());
            return path;
        }

        public static Guid GetEntryIdFromUrl(this HttpContext ctx, string url)
        {
            Uri requestUrl = new Uri(url);
            var path = requestUrl.PathAndQuery;
            

            if (path.Contains(@"/entries/"))
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

        public static string MediaUrl(this HttpContext ctx, Media e)
        {
            var path = ctx.Request.Scheme + "://" +  Path.Join(ctx.Request.Host.ToString(), ctx.Request.PathBase, "media", e.Id.ToString());
            return path;
        }

        public static Guid GetMediaIdFromUrl(this HttpContext ctx, string url)
        {
            Uri requestUrl = new Uri(url);
            var path = requestUrl.PathAndQuery;

            if (path.Contains(@"/media/"))
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
