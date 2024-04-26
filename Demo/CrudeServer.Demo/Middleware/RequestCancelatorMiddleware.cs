using System;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Demo.Middleware
{
    public class RequestCancelatorMiddleware : IMiddleware
    {
        public async Task Process(ICommandContext context, Func<Task> next)
        {
            if (context.RequestUrl.AbsolutePath.Contains("teapot"))
            {
                context.Response = new StatusCodeResponse()
                {
                    StatusCode = 418
                };

                return;
            }

            await next();
        }
    }
}
