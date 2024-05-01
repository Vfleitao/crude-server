using System;
using System.Linq;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class CommandRetrieverMiddleware : IMiddleware
    {
        private readonly ICommandRegistry _commandRegistry;
        private readonly ILogger loggerProvider;

        public CommandRetrieverMiddleware(
            ICommandRegistry commandRegistry,
            ILogger loggerProvider
        )
        {
            this._commandRegistry = commandRegistry;
            this.loggerProvider = loggerProvider;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
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
                this.loggerProvider.Log($"[CommandRetrieverMiddleware] Command not found for {context.RequestUrl.AbsolutePath}");

                httpResponse = new NotFoundResponse();
            }
            else if (commandRegistration.RequiresAuthentication && !IsUserAuthenticated(commandRegistration, context))
            {
                this.loggerProvider.Log($"[CommandRetrieverMiddleware] Command is unauthorized for {context.RequestUrl.AbsolutePath}");

                httpResponse = new UnauthorizedResponse();
            }

            context.Response = httpResponse;

            await next();
        }

        private bool IsUserAuthenticated(HttpCommandRegistration commandRegistration, ICommandContext context)
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

        private bool UserIsLoggedIn(ICommandContext context)
        {
            return context.User != null &&
                   context.User.Identity != null &&
                   context.User.Identity.IsAuthenticated;
        }
    }
}
