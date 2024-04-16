using System;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Lib.Tests.Mocks
{
    public class MockMiddleware : IMiddleware
    {
        public Task Process(IRequestContext context, Func<Task> next)
        {
            throw new NotImplementedException();
        }
    }
}
