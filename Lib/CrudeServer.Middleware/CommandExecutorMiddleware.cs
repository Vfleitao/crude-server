using System;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.MiddlewareRegistration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

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
            HttpMethod httpMethod = HttpMethodExtensions.FromHttpString(context.HttpRequest.HttpMethod.ToUpper());

            HttpCommandRegistration commandRegistration = _commandRegistry.GetCommand(
                context.HttpRequest.Url.AbsolutePath,
                httpMethod
            );

            context.HttpRegistration = commandRegistration;

            IHttpResponse httpResponse = null;
            if (commandRegistration == null)
            {
                httpResponse = new NotFoundResponse();
            }
            else if (commandRegistration.RequiresAuthentication && context.User == null)
            {
                httpResponse = new UnauthorizedResponse();
            }
            else
            {
                HttpCommand command = (HttpCommand)context.Services.GetService(commandRegistration.Command);
                command.SetContext(context.HttpContext);

                httpResponse = await command.Process();
            }

            context.Response = httpResponse;

            await next();
        }
    }
}
