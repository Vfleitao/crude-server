using System.Net;
using System.Threading.Tasks;

namespace CrudeServer.Server.Contracts
{
    public interface IHttpRequestExecutor
    {
        Task Execute(HttpListenerContext context);
    }
}