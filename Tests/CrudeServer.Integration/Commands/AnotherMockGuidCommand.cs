using System;
using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Integration.Commands
{
    public class AnotherMockGuidCommand : HttpCommand
    {
        public AnotherMockGuidCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            return await View("simple.html", new
            {
                value = "Heya " + Guid.NewGuid()
            });
        }
    }
}
