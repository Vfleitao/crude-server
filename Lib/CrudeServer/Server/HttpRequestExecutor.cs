﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server
{
    public class HttpRequestExecutor : IHttpRequestExecutor
    {
        private readonly IMiddlewareRegistry _middlewareRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger loggerProvider;
        private readonly ICommandContext commandContext;

        public HttpRequestExecutor(
            IMiddlewareRegistry middlewareRegistry,
            IServiceProvider serviceProvider,
            ILogger loggerProvider,
            ICommandContext commandContext
        )
        {
            this._middlewareRegistry = middlewareRegistry;
            this._serviceProvider = serviceProvider;
            this.loggerProvider = loggerProvider;
            this.commandContext = commandContext;
        }

        public async Task Execute(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                List<Type> middlewareTypes = this._middlewareRegistry.GetMiddlewares().ToList();

                if (!middlewareTypes.Any())
                {
                    throw new InvalidOperationException("No middleware registered.");
                }

                Func<Task> endOfChain = () => Task.CompletedTask;
                Func<Task> executionChain = endOfChain;

                commandContext.ConfigureContext(context, request, response);

                IEnumerable<Type> reversedList = middlewareTypes.AsReadOnly().Reverse();
                foreach (Type type in reversedList)
                {
                    executionChain = BuildNextItemInChain(_serviceProvider, type, executionChain, commandContext);
                }

                await executionChain();

                IMiddleware responseProcessor = this._serviceProvider.GetKeyedService<IMiddleware>(ServerConstants.RESPONSE_PROCESSOR);
                await responseProcessor.Process(commandContext, endOfChain);

                response.Close();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debugger.Break();
#endif

                this.loggerProvider.Error(ex);

                try
                {
                    response.OutputStream.SetLength(0);

                }
                catch (Exception)
                {
                    /// This should never be hit, but we will ignore it if it does
                    /// Since we do nto want to throw an exception in the catch block
                }
                finally
                {
                    response.StatusCode = 500;
                    response.Close();
                }
            }
        }

        private Func<Task> BuildNextItemInChain(IServiceProvider serviceProvider, Type middlewareType, Func<Task> next, ICommandContext context)
        {
            return async () =>
            {
                loggerProvider.Log($"[HttpRequestExecutor] Executing middleware {middlewareType.Name}");

                IMiddleware middleware = (IMiddleware)serviceProvider.GetService(middlewareType);
                await middleware.Process(context, next);

                loggerProvider.Log($"[HttpRequestExecutor] Executed middleware {middlewareType.Name}");
            };
        }
    }
}
