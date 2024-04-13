using System.Net;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.HttpCommands
{
    public abstract class HttpCommand
    {
        protected HttpListenerRequest Request { get; private set; }
        protected HttpListenerResponse Response { get; private set; }

        public void SetContext(HttpListenerContext context)
        {
            Request = context.Request;
            Response = context.Response;
        }

        public async virtual Task<IHttpResponse> HandleRequest()
        {
            return await Process();
        }

        protected abstract Task<IHttpResponse> Process();
    }
}
