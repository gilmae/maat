using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;


namespace SV.Maat.Micropub.Models
{
    public class QueryModel
    {
        [BindProperty(Name ="q")]
        public string Query { get; set; }

        [BindProperty(Name ="url")]
        public string Url { get; set; }

        [BindProperty(Name ="properties[]")]
        public string[] Properties { get; set; }

        [BindProperty(Name="limit")]
        public int? Limit { get; set; }

        [BindProperty(Name = "after")]
        public string After { get; set; }

        [BindProperty(Name = "before")]
        public string Before { get; set; }
    }

    public class QueryType
    {
        public static readonly string config = "config";
        public static readonly string source = "source";
        public static readonly string syndicateTo = "syndicate-to";
        public static readonly string replies = "replies";
    }
}
