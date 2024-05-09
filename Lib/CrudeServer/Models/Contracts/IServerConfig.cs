using System.Collections.Generic;

namespace CrudeServer.Models.Contracts
{
    public interface IServerConfig
    {
        string AuthenticationPath { get; set; }
        List<string> Hosts { get; set; }
        string NotFoundPath { get; set; }
        bool RedirectOnNotFound { get; set; }
        bool RedirectOnAjaxCalls { get; set; }
        JTWConfig JTWConfiguration { get; set; }
        long CachedDurationMinutes { get; set; }
        bool EnableServerFileCache { get; set; }
        string PrivateEncryptionKey { get; set; }
        string PublicEncryptionKey { get; set; }
        string AntiforgeryTokenCookieName { get; set; }
        string AntiforgeryTokenInputName { get; set; }
        long MaxRequestSizeMB { get; set; }
    }
}