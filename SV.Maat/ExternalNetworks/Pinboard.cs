using System;
using StrangeVanilla.Blogging.Events;
using pinboard.net;
using pinboard.net.Models;
using SV.Maat.lib;

namespace SV.Maat.ExternalNetworks
{
    public class Pinboard : ISyndicationNetwork, IRequiresCredentialEntry
    {
        public Pinboard()
        {
        }

        public string Name => "Pinboard";

        public string Photo => "https://pinboard.in/bluepin.gif";

        public string Url => "https://pinboard.in";

        public string Syndicate(Credentials credentials, Entry entry)
        {
            using (var pb = new PinboardAPI($"{credentials.Uid}:{credentials.Secret}"))
            {
                Bookmark bookmark = new Bookmark();
                bookmark.Url = entry.BookmarkOf;
                bookmark.Description = ContentHelper.GetPlainText(entry.Title);
                bookmark.Extended = ContentHelper.GetPlainText(entry.Body);
                bookmark.Tags.AddRange(entry.Categories);

                var result = pb.Posts.Add(bookmark).Result;

                if (result.ResultCode)
                {
                    return $"pinboard:{System.Web.HttpUtility.UrlEncode(entry.BookmarkOf)}";
                }
                return "";
            }
        }
    }
}
