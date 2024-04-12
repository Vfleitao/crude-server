namespace CrudeServer.Models.Contracts
{
    public interface IServerConfig
    {
        string AuthenticationPath { get; set; }
        string Host { get; set; }
        string Port { get; set; }
        string NotFoundPath { get; }
        bool RedirectOnAjaxCalls { get; set; }
    }
}