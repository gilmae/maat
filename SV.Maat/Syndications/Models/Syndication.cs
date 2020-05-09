using System;
using SV.Maat.lib.Repository;

namespace SV.Maat.Syndications.Models
{
    public class Syndication : Model
    {
        public string AccountName { get; set; }
        public string Network { get; set; }
        public int UserId { get; set; }
    }
}
