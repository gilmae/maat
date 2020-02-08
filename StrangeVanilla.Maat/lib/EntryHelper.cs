using System;
using System.IO;
using StrangeVanilla.Blogging.Events;

namespace StrangeVanilla.Maat.lib
{
    public static class EntryHelper
    {
        public static string EntryUrl(this Nancy.NancyContext ctx, Entry e)
        {
            var url = ctx.Request.Url;
            var path = Path.Join(url.SiteBase, "entries", e.Id.ToString());
            return path;
        }

        public static Guid GetEntryIdFromUrl(this Nancy.NancyContext ctx)
        {
            var url = ctx.Request.Url;

            var path = url.Path;

            if (path.StartsWith("entries"))
            {
                var potentialId = path.Replace(@"entries/", "");

                Guid entryId;

                if (Guid.TryParse(potentialId, out entryId))
                {
                    return entryId;
                }
            }

            return Guid.Empty;
        }
    }
}
