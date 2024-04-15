using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Lib.Tests.Mocks
{
    public class MockHttpResponse : IHttpResponse
    {
        public byte[] ResponseData { get; set; }
        public string ContentType { get; set; }
        public int StatusCode { get; set; }
        public object ViewData { get; set; }
        public IDictionary<string, object> Items { get; set; }

        public Task ProcessResponse()
        {
            return Task.CompletedTask;
        }
    }
}