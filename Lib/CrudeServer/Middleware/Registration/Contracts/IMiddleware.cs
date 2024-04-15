using System;
using System.Threading.Tasks;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware.Registration.Contracts
{
    public interface IMiddleware
    {
        Task Process(IRequestContext context, Func<Task> next);
    }
}
