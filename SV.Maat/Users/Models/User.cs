using System;
using SV.Maat.lib.Repository;

namespace SV.Maat.Users.Models
{
    public class User : Model
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string HashedPassword { get; set; }
    }
}
