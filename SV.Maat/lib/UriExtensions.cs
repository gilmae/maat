using System;
using System.Linq;

namespace SV.Maat.lib
{
    public static class UriExtensions
    {
        public static Uri RemoveDocument(this Uri uri)
        {
            
            var parts = uri.ToString().Split("/");
            if (parts.Last().Contains("."))
            {
                parts = parts.Take(parts.Length - 1).ToArray();
            }
            return new Uri(string.Join('/', parts));
        }

        public static string GetPathOfResourceRelativeToBase(this Uri absolute, string relative)
        {
            var uriWithoutDocument = absolute.RemoveDocument();
            return new Flurl.Url(uriWithoutDocument).AppendPathSegment(relative);
        }

        public static string GetPathOfResourceRelativeToBase(this string absolute, string relative)
        {
            var uriWithoutDocument = new Uri(absolute).RemoveDocument();
            return new Flurl.Url(uriWithoutDocument).AppendPathSegment(relative);
        }

        public static bool IsAbsoluteUri(this string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }
    }
}
