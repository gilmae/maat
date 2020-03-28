using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SV.Maat.Micropub.Models
{
    public class QueryModel
    {
        [BindProperty(Name ="q")]
        public string Q { get; set; }

        [BindProperty(Name ="url")]
        public string Url { get; set; }

        [BindProperty(Name ="properties[]")]
        public string[] Properties { get; set; }
    }


    public enum QueryType
    {
        config,
        source
    }
}
