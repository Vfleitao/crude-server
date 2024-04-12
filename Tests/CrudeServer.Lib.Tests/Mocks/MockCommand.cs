using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Lib.Tests.Mocks
{
    public class MockCommand : HttpCommand
    {
        protected override Task<IHttpResponse> Process()
        {
            return Task.FromResult<IHttpResponse>(new MockHttpResponse());
        }
    }
}
