using System.Threading.Tasks;

using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;

namespace CrudeServer
{
    public class InDepthRedirectCommand : BaseCommand
    {
        protected override async Task<IHttpResponse> Process()
        {
            return new RedirectResponse("/in-depth/start", 301);
        }
    }
}
