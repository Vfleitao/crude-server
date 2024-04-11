using System.Net;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.HttpCommands
{
    public abstract class HttpCommand
    {
        protected HttpListenerContext RequestContext { get; private set; }
        protected HttpListenerRequest Request { get; private set; }
        protected HttpListenerResponse Response { get; private set; }

        internal void SetContext(HttpListenerContext context)
        {
            RequestContext = context;
            Request = context.Request;
            Response = context.Response;
        }

        public abstract Task<IHttpResponse> Process();
    }
}
