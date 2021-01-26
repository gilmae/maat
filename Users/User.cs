using SimpleRepo;

namespace Users
{
    public class User : Model
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string HashedPassword { get; set; }
        public string Host { get; set; }

        public string Bio { get; set; }
    }
}
