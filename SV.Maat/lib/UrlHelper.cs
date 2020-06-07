using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StrangeVanilla.Blogging.Events;

namespace SV.Maat.lib
{
    public static class UrlHelper
    {
        public static string EntryUrl(this HttpContext ctx, Entry e)
        {
            return EntryUrl(ctx, e.Id);
        }

        public static string EntryUrl(this HttpContext ctx, Guid id)
        {
            var path = ctx.Request.Scheme + "://" + Path.Join(ctx.Request.Host.ToString(), ctx.Request.PathBase, "entries", id.ToString());
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

        public static string MediaUrl(this IUrlHelper ctx, Media e)
        {
            return ctx.ActionLink("GetMediaFile", "Media", new { id = e.Id });
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

        public static int GetUserIdFromUrl(this IUrlHelper ctx, string url)
        {
            var genericUserUrl = ctx.ActionLink("view", "users", new { id = -1 }).ToLower();

            string prefix = genericUserUrl.Substring(0, genericUserUrl.IndexOf("-1"));

            string possibleId = url.ToLower().Replace(prefix, "");

            if (possibleId.Contains("/"))
            {
                possibleId = possibleId.Substring(0, possibleId.IndexOf("/"));
            }

            return int.Parse(possibleId);
        }
    }
}
