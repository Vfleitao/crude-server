using System;
using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Integration.Commands
{
    public class MockGuidHttpCommand : HttpCommand
    {
        public MockGuidHttpCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            return await View("simple.html", new
            {
                value = "Yoh " + Guid.NewGuid()
            });
        }
    }
}
