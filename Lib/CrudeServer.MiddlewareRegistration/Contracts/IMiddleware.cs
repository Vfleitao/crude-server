using System;
using System.Threading.Tasks;

using CrudeServer.Models;

namespace CrudeServer.MiddlewareRegistration.Contracts
{
    public interface IMiddleware
    {
        Task Process(RequestContext context, Func<Task> next);
    }
}
