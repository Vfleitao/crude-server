using System;
using System.Collections.Generic;

using CrudeServer.MiddlewareRegistration.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.MiddlewareRegistration
{
    public class MiddlewareRegistry : IMiddlewareRegistry
    {
        private readonly List<Type> _middlewareTypes = new List<Type>();
        private readonly IServiceCollection _services;

        public MiddlewareRegistry(IServiceCollection services)
        {
            this._services = services;
        }

        public void AddMiddleware<T>() where T : IMiddleware
        {
            if (_middlewareTypes.Contains(typeof(T)))
            {
                throw new InvalidOperationException($"Middleware of type {typeof(T).Name} is already registered.");
            }

            _middlewareTypes.Add(typeof(T));
            _services.AddSingleton(typeof(T));
        }

        public IEnumerable<Type> GetMiddlewares()
        {
            return _middlewareTypes;
        }

    }
}
