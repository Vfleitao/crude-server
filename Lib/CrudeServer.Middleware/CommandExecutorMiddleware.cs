using System;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.MiddlewareRegistration.Contracts;
using CrudeServer.Models;

namespace CrudeServer.Middleware
{
    public class CommandExecutorMiddleware : IMiddleware
    {
        private readonly ICommandRegistry _commandRegistry;

        public CommandExecutorMiddleware(ICommandRegistry commandRegistry)
        {
            this._commandRegistry = commandRegistry;
        }

        public async Task Process(RequestContext context, Func<Task> next)
        {
            HttpMethod httpMethod = HttpMethodExtensions.FromHttpString(context.Request.HttpMethod.ToUpper());

            HttpCommandRegistration commandRegistration = _commandRegistry.GetCommand(
                context.Request.Url.AbsolutePath,
                httpMethod
            );

            IHttpResponse httpResponse;
            if (commandRegistration == null)
            {
                httpResponse = new NotFoundResponse();
            }
            else
            {
                HttpCommand command = (HttpCommand)context.Services.GetService(commandRegistration.Command);
                command.SetContext(context.Context);

                httpResponse = await command.Process();
            }

            context.Response.StatusCode = httpResponse.StatusCode;
            context.Response.ContentType = httpResponse.ContentType;

            await context.Response.OutputStream.WriteAsync(httpResponse.ResponseData, 0, httpResponse.ResponseData.Length);
        }
    }
}
