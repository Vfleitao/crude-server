using System.Collections.Generic;

using CrudeServer.Models.Contracts;

namespace CrudeServer.Models
{
    public class ServerConfig : IServerConfig
    {
        public List<string> Hosts { get; set; }
        public string AuthenticationPath { get; set; }
        public string NotFoundPath { get; set; }
        public bool RedirectOnAjaxCalls { get; set; }
        public JTWConfig JTWConfiguration { get; set; }
        public long CachedDurationMinutes { get; set; }
        public bool EnableServerFileCache { get; set; }
        public string PrivateEncryptionKey { get; set; }
        public string PublicEncryptionKey { get; set; }
        public string AntiforgeryTokenCookieName { get; set; }
        public string AntiforgeryTokenInputName { get; set; }
        public long MaxRequestSizeMB { get; set; }
        public bool RedirectOnNotFound { get; set; }
    }
}
