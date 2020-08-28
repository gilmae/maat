using System;
namespace SV.Maat.Entries.Models
{
    public struct Entry
    {
        public string Title;
        public string Body;
        public string[] Categories;
        public Media[] Photos;
        public DateTime PublishedAt;
        public string Bookmark;
    }

    public struct Media
    {
        public string Url;
        public string Description;
    }
}
