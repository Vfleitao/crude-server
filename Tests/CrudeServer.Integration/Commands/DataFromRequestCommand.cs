using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Integration.Commands
{
    public class DataFromRequestCommand : HttpCommand
    {
        public DataFromRequestCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            return new JsonResponse(new
            {
                items = RequestContext.Items,
                files = RequestContext.Files,
            });
        }
    }
}
