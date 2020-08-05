using System;
using System.IO;
using System.Linq;
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

        public static Guid GetEntryIdFromUrl(this Uri entryUrl)
        {
            // EntryUrl will be in the form scheme://host/entries/{entryId}.
            // It should not include a querystring, but get rid of one if it is there
            var path = entryUrl.PathAndQuery.Split('?').FirstOrDefault();

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

        public static string GetUserNameFromUrl(this IUrlHelper ctx, string url)
        {
            // User urls are in the format <host>/user/{username}

            Uri uri = new Uri(url);
            var parts = uri.AbsolutePath.Split("/").Where(p => !string.IsNullOrEmpty(p)).ToArray();

            string username = parts[1];

            return username;
        }
    }
}
