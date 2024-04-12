using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.HttpCommands.Responses
{
    public class StatusCodeResponse : IHttpResponse
    {
        public virtual byte[] ResponseData { get; set; } = [];
        public virtual string ContentType { get; set; } = "text/html";
        public virtual int StatusCode { get; set; }

        public virtual Task ProcessResponse()
        {
            return Task.CompletedTask;
        }
    }
}
