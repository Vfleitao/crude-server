using System;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware
{
    public class CommandExecutorMiddleware : IMiddleware
    {
        private readonly ICommandRegistry _commandRegistry;
        private readonly IServiceProvider serviceProvider;

        public CommandExecutorMiddleware(ICommandRegistry commandRegistry, IServiceProvider serviceProvider)
        {
            this._commandRegistry = commandRegistry;
            this.serviceProvider = serviceProvider;
        }

        public async Task Process(IRequestContext context, Func<Task> next)
        {
            HttpMethod httpMethod = context.HttpMethod;

            HttpCommandRegistration commandRegistration = _commandRegistry.GetCommand(
                context.Url.AbsolutePath,
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
                HttpCommand command = (HttpCommand)this.serviceProvider.GetService(commandRegistration.Command);
                command.SetContext(context);

                httpResponse = await command.ExecuteRequest();
            }

            context.Response = httpResponse;

            await next();
        }
    }
}
