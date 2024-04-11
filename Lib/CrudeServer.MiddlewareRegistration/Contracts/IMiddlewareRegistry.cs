using System;
using System.Collections.Generic;

namespace CrudeServer.MiddlewareRegistration.Contracts
{
    public interface IMiddlewareRegistry
    {
        void AddMiddleware<T>() where T : IMiddleware;
        IEnumerable<Type> GetMiddlewares();
    }
}