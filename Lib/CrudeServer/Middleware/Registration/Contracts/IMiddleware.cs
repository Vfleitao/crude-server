using System;
using System.Threading.Tasks;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware.Registration.Contracts
{
    public interface IMiddleware
    {
        Task Process(ICommandContext context, Func<Task> next);
    }
}
