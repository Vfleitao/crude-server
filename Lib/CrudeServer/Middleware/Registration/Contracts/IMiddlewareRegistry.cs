using System;
using System.Collections.Generic;

namespace CrudeServer.Middleware.Registration.Contracts
{
    public interface IMiddlewareRegistry
    {
        void AddMiddleware<T>() where T : IMiddleware;
        IEnumerable<Type> GetMiddlewares();
    }
}