namespace CrudeServer.Models
{
    public class HttpCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
        public string Domain { get; set; }
        public long ExpireTimeMinutes { get; set; }
        public bool Secure { get; set; } = true;
        public bool HttpOnly { get; set; }
    }
}
