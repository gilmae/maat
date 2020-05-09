using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace SV.Maat.Syndications.Models
{
    public class RegisterSyndicationModel
    {
        [BindProperty(Name = "name")]
        public string  Name { get; set; }

        [BindProperty(Name = "network")]
        public SyndicationTarget Network { get; set; }
    }
}
