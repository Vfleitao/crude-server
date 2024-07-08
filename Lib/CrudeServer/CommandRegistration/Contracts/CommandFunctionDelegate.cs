using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.CommandRegistration.Contracts
{
    public delegate Task<IHttpResponse> CommandFunctionDelegate(ICommandContext commandContext);
}
