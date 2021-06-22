using SimpleDbRepository;

namespace SV.Maat.Syndications.Models
{
    public class Syndication : Model
    {
        public string AccountName { get; set; }
        public string Network { get; set; }
        public int UserId { get; set; }
        public string Credentials { get; set; }
    }
}
