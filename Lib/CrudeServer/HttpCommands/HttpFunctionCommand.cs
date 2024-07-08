using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.HttpCommands
{
    public class HttpFunctionCommand : HttpCommand
    {
        public CommandFunctionDelegate DelegateFunction { get; set; }

        public HttpFunctionCommand(ICommandContext requestContext)
            : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            return await DelegateFunction(RequestContext);
        }
    }
}
