using System;
namespace SV.Maat.Entries.Models
{
    public struct Entry
    {
        public string Title;
        public string Body;
        public string[] Categories;
        public string[] Photos;
        public DateTime PublishedAt;
    }
}
