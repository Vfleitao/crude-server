using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class RequestTaggerMiddleware(ILogger loggerProvider) : IMiddleware
    {
        public async Task Process(ICommandContext context, Func<Task> next)
        {
            if (context.ResponseHeaders == null)
            {
                context.ResponseHeaders = new Dictionary<string, string>();
            }

            loggerProvider.Log($"[10] Tagging Request");
            context.ResponseHeaders.Add("X-Request-Id", Guid.NewGuid().ToString());

            await next();
        }
    }
}
