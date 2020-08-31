using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StrangeVanilla.Blogging.Events;
using Users;

namespace SV.Maat.lib
{
    public static class UrlHelper
    {
        public static string EntryUrl(Entry e, User u)
        {
            string host = u?.Host;
            if (string.IsNullOrEmpty(host))
            {
                host = "/";
            }

            string slug = e.Slug;

            if (string.IsNullOrEmpty(slug))
            {
                slug = $"entries/{e.Id}";
            }
            return Path.Join(host, slug);
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

            return Regex.Match(uri.AbsolutePath, @"user\/([^/?]+)", RegexOptions.IgnoreCase)?.Groups[1].Value;

        }
    }
}
