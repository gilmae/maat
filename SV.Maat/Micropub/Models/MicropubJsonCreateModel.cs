using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace SV.Maat.Micropub.Models
{
    public class MicropubPublishModel
    {
        [JsonPropertyName("type")]
        [BindProperty(Name = "type")]
        public string[] Type { get; set; }

        [JsonPropertyName("properties")]
        [BindProperty(Name="properties")]
        public Dictionary<string, dynamic[]> Properties { get; set; }

        [BindProperty(Name = "action")]
        public string Action { get; set; }

        [BindProperty(Name = "url")]
        public string Url { get; set; }

        [JsonPropertyName("add")]
        [BindProperty(Name = "add")]
        public Dictionary<string, string[]> Add { get; set; }

        [JsonPropertyName("remove")]
        [BindProperty(Name = "remove")]
        public string[] Remove { get; set; }

        [JsonPropertyName("replace")]
        [BindProperty(Name = "replace")]
        public Dictionary<string, string[]> Replace { get; set; }
    }

    public static class MicropubPublisModelExtensions
    {
        public static bool IsCreate(this MicropubPublishModel model)
        {
            return model.Type != null && model.Type.Count() > 0;
        }
    }
}
