using System;
using System.Collections.Generic;

using CrudeServer.Middleware.Registration.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Middleware.Registration
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
            _services.AddScoped(typeof(T));
        }

        public IEnumerable<Type> GetMiddlewares()
        {
            return _middlewareTypes;
        }

        public bool ContainsMiddleware<T>() where T : IMiddleware
        {
            return _middlewareTypes.Contains(typeof(T));
        }
    }
}
