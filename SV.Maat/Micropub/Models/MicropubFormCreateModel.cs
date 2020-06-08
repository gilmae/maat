using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SV.Maat.Micropub.Models
{
    [DataContract]
    public class MicropubFormCreateModel
    {
        [BindProperty(Name = "h")]
        public string Type { get; set; }

        [BindProperty(Name = "content")]
        public string Content { get; set; }

        [BindProperty(Name = "category[]")]
        public string[] Categories { get; set; }

        [BindProperty(Name = "name")]
        public string Title { get; set; }

        [BindProperty(Name = "bookmark-of")]
        public string BookmarkOf { get; set; }

        [BindProperty(Name = "post-status")]
        public string PostStatus { get; set; }

        [BindProperty(Name = "in-reply-to")]
        public string ReplyTo { get; set; }

        [BindProperty(Name = "photo")]
        public IFormFile Photo { get; set; }

        [BindProperty(Name = "mp-syndicate-to")]
        public string[] SyndicateTo { get; set; }
    }
}