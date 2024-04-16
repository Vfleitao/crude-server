using System;
using System.Linq;
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
        private readonly IServiceProvider _serviceProvider;

        public CommandExecutorMiddleware(ICommandRegistry commandRegistry, IServiceProvider serviceProvider)
        {
            this._commandRegistry = commandRegistry;
            this._serviceProvider = serviceProvider;
        }

        public async Task Process(IRequestContext context, Func<Task> next)
        {
            HttpMethod httpMethod = context.RequestHttpMethod;

            HttpCommandRegistration commandRegistration = _commandRegistry.GetCommand(
                context.RequestUrl.AbsolutePath,
                httpMethod
            );

            context.HttpRegistration = commandRegistration;

            IHttpResponse httpResponse = null;
            if (commandRegistration == null)
            {
                httpResponse = new NotFoundResponse();
            }
            else if (commandRegistration.RequiresAuthentication && !IsUserAuthenticated(commandRegistration, context))
            {
                httpResponse = new UnauthorizedResponse();
            }
            else
            {
                HttpCommand command = (HttpCommand)this._serviceProvider.GetService(commandRegistration.Command);
                command.SetContext(context);

                httpResponse = await command.ExecuteRequest();
            }

            context.Response = httpResponse;

            await next();
        }

        private bool IsUserAuthenticated(HttpCommandRegistration commandRegistration, IRequestContext context)
        {
            if (!UserIsLoggedIn(context))
            {
                return false;
            }

            if (commandRegistration.AuthenticationRoles == null || !commandRegistration.AuthenticationRoles.Any())
            {
                return true;
            }

            return commandRegistration.AuthenticationRoles.Any(x => context.User.IsInRole(x));
        }

        private bool UserIsLoggedIn(IRequestContext context)
        {
            return context.User != null &&
                   context.User.Identity != null &&
                   context.User.Identity.IsAuthenticated;
        }
    }
}
