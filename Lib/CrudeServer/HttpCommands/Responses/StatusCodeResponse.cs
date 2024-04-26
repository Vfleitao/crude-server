using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models;

namespace CrudeServer.HttpCommands.Responses
{
    public class StatusCodeResponse : IHttpResponse
    {
        public virtual byte[] ResponseData { get; set; } = [];
        public virtual string ContentType { get; set; } = "text/html";
        public virtual int StatusCode { get; set; }
        public object ViewData { get; set; }
        public IDictionary<string, object> Items { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public IEnumerable<HttpCookie> Cookies { get; set; } = new List<HttpCookie>();

        public virtual Task ProcessResponse()
        {
            return Task.CompletedTask;
        }
    }
}
