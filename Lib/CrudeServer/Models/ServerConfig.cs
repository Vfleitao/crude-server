using CrudeServer.Models.Contracts;

namespace CrudeServer.Models
{
    public class ServerConfig : IServerConfig
    {
        public string Port { get; set; }
        public string Host { get; set; }
        public string AuthenticationPath { get; set; }
        public string NotFoundPath { get; }
        public bool RedirectOnAjaxCalls { get; set; }
        public JTWConfig JTWConfiguration { get; set; }
        public long CachedDurationMinutes { get; set; }
    }
}
