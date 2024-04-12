using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Lib.Tests.Mocks
{
    public class MockCommand : HttpCommand
    {
        public override Task<IHttpResponse> Process()
        {
            return Task.FromResult<IHttpResponse>(new MockHttpResponse());
        }
    }
}
