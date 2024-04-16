using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware
{
    public class RequestTaggerMiddleware : IMiddleware
    {

        public RequestTaggerMiddleware()
        {
        }

        public async Task Process(IRequestContext context, Func<Task> next)
        {
            if (context.ResponseHeaders == null)
            {
                context.ResponseHeaders = new Dictionary<string, string>();
            }

            context.ResponseHeaders.Add("X-Request-Id", Guid.NewGuid().ToString());

            await next();
        }
    }
}
