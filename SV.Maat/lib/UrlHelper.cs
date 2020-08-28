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
            if (string.IsNullOrEmpty(e.Slug))
            {
                return $"{ctx.Request.Scheme}://{ctx.Request.Host}/entries/{e.Id}";
            }
            return $"{ctx.Request.Scheme}://{ctx.Request.Host}/{e.Slug}";
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

        public static string GetUserNameFromUrl(this Uri uri)
        {
            // User urls are in the format <host>/user/{username}

            var path = uri.AbsolutePath;

            if (path.Contains(@"/user/"))
            {
                var potentialUserName = path.Replace(@"/user/", "");

                return potentialUserName;
            }

            return "";
        }
    }
}
