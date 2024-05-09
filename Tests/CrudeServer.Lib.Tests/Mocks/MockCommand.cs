using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Lib.Tests.Mocks
{
    public class MockCommand : HttpCommand
    {
        public MockCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override Task<IHttpResponse> Process()
        {
            return Task.FromResult<IHttpResponse>(new MockHttpResponse());
        }
    }
}
