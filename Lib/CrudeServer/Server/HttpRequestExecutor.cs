using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Server
{
    public class HttpRequestExecutor : IHttpRequestExecutor
    {
        private readonly IMiddlewareRegistry _middlewareRegistry;
        private readonly IServiceProvider _serviceProvider;

        public HttpRequestExecutor(
            IMiddlewareRegistry middlewareRegistry,
            IServiceProvider serviceProvider)
        {
            this._middlewareRegistry = middlewareRegistry;
            this._serviceProvider = serviceProvider;
        }

        public async Task Execute(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                IRequestContext requestContext = new RequestContext(
                    context,
                    request,
                    response,
                    this._serviceProvider
                );

                List<Type> middlewareTypes = this._middlewareRegistry.GetMiddlewares().ToList();

                if (!middlewareTypes.Any())
                {
                    throw new InvalidOperationException("No middleware registered.");
                }

                Func<Task> endOfChain = () => Task.CompletedTask;
                Func<Task> executionChain = endOfChain;

                IEnumerable<Type> reversedList = middlewareTypes.AsReadOnly().Reverse();
                foreach (Type type in reversedList)
                {
                    executionChain = BuildNextItemInChain(_serviceProvider, type, executionChain, requestContext);
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                await executionChain();

                stopwatch.Stop();
                Console.WriteLine($"executionChain Took {stopwatch.ElapsedMilliseconds}ms");

                response.Close();
            }
            catch (Exception e)
            {
                response.OutputStream.SetLength(0);
                response.StatusCode = 500;
            }
        }

        private Func<Task> BuildNextItemInChain(IServiceProvider serviceProvider, Type middlewareType, Func<Task> next, IRequestContext context)
        {
            return async () =>
            {
                IMiddleware middleware = (IMiddleware)serviceProvider.GetService(middlewareType);

                await middleware
                        .Process(context, next)
                        .ConfigureAwait(false);
            };
        }
    }
}
