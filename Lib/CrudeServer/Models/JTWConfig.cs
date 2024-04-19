namespace CrudeServer.Models
{
    public class JTWConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SigningKey { get; set; }
        public long ExpiresAfter { get; set; }
    }
}
