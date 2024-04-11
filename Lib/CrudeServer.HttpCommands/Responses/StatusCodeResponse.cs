using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.HttpCommands.Responses
{
    public class StatusCodeResponse : IHttpResponse
    {
        public byte[] ResponseData { get; set; } = [];
        public string ContentType { get; set; } = "text/html";
        public virtual int StatusCode { get; set; }

        public virtual Task ProcessResponse()
        {
            return Task.CompletedTask;
        }
    }
}
