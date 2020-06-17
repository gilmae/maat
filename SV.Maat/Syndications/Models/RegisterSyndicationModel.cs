using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace SV.Maat.Syndications.Models
{
    public class RegisterSyndicationModel
    {
        [BindProperty(Name = "network")]
        public string Network { get; set; }
    }
}
