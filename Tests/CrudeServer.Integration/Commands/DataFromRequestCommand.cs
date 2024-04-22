using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;

namespace CrudeServer.Integration.Commands
{
    public class DataFromRequestCommand : HttpCommand
    {
        protected override async Task<IHttpResponse> Process()
        {
            return new OkResponse();
        }
    }
}
