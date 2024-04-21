using System.Collections.Generic;

using CrudeServer.Models.Contracts;

namespace CrudeServer.Models
{
    public class ServerConfig : IServerConfig
    {
        public List<string> Hosts { get; set; }
        public string AuthenticationPath { get; set; }
        public string NotFoundPath { get; }
        public bool RedirectOnAjaxCalls { get; set; }
        public JTWConfig JTWConfiguration { get; set; }
        public long CachedDurationMinutes { get; set; }
    }
}
