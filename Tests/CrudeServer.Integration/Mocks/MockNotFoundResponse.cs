using System.Text;

using CrudeServer.HttpCommands.Responses;

namespace CrudeServer.Integration.Mocks
{
    public class MockNotFoundResponse : StatusCodeResponse
    {
        public MockNotFoundResponse()
        {
            this.StatusCode = 404;
            this.ResponseData = Encoding.UTF8.GetBytes("THIS IS A CUSTOM NOT FOUND PAGE");
        }
    }
}
