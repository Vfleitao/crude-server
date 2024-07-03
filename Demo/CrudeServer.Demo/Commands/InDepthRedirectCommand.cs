using System.Threading.Tasks;

using CrudeServer.Attributes;
using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models.Contracts;

namespace CrudeServer
{
    [Command("/in-depth")]
    public class InDepthRedirectCommand : BaseCommand
    {
        public InDepthRedirectCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            return new RedirectResponse("/in-depth/start", 301);
        }
    }
}
