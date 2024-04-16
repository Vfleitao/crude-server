using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Integration.Commands
{
    public class AnotherMockGuidCommand : HttpCommand
    {
        protected override async Task<IHttpResponse> Process()
        {
            return await View("simple.html", new
            {
                value = "Heya " + Guid.NewGuid()
            });
        }
    }
}
