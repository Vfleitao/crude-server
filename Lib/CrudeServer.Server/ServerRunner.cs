using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.MiddlewareRegistration.Contracts;
using CrudeServer.Models;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Server
{
    public class ServerRunner : IServerRunner
    {
        private static int requestCount = 0;

        private readonly HttpListener _listener;

        private readonly ServerConfig _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMiddlewareRegistry _middlewareRegistry;

        public ServerRunner(
            IServiceProvider serviceProvider,
            IMiddlewareRegistry middlewareRegistry,
            ServerConfig Configuration)
        {
            this._serviceProvider = serviceProvider;
            this._middlewareRegistry = middlewareRegistry;
            this._configuration = Configuration;

            _listener = new HttpListener();
        }

        public async Task Run()
        {
            if (this._serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not set. Please call Build() before running the server.");
            }

            _listener.Prefixes.Add($"{this._configuration.Host}:{this._configuration.Port}/");
            _listener.Start();

            Console.WriteLine("Listening for connections on {0}:{1}", this._configuration.Host, this._configuration.Port);
            try
            {
                while (true)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    HttpListenerRequest req = context.Request;
                    HttpListenerResponse resp = context.Response;

                    try
                    {
                        RequestContext requestContext = new RequestContext(context, req, resp, this._serviceProvider);

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
                            executionChain = BuildNext(_serviceProvider, type, executionChain, requestContext);
                        }

                        await executionChain();

                        resp.Close();
                    }
                    catch (Exception e)
                    {
                        resp.OutputStream.SetLength(0);
                        resp.StatusCode = 500;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _listener.Close();
            }
        }

        private Func<Task> BuildNext(IServiceProvider serviceProvider, Type middlewareType, Func<Task> next, RequestContext context)
        {
            return async () =>
            {
                IMiddleware middleware = (IMiddleware)serviceProvider.GetService(middlewareType);
                await middleware.Process(context, next);
            };
        }
    }
}
