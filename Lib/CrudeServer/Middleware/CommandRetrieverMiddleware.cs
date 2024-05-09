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
        private readonly IStandardResponseRegistry standardResponseProvider;
        private readonly IServiceProvider serviceProvider;

        public CommandRetrieverMiddleware(
            ICommandRegistry commandRegistry,
            ILogger loggerProvider,
            IStandardResponseRegistry standardResponseProvider,
            IServiceProvider serviceProvider
        )
        {
            this._commandRegistry = commandRegistry;
            this.loggerProvider = loggerProvider;
            this.standardResponseProvider = standardResponseProvider;
            this.serviceProvider = serviceProvider;
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

                Type notFoundResponseType = standardResponseProvider.GetResponseType(DefaultStatusCodes.NotFound);
                httpResponse = (IHttpResponse)this.serviceProvider.GetService(notFoundResponseType);
            }
            else if (commandRegistration.RequiresAuthentication && !IsUserAuthenticated(commandRegistration, context))
            {
                this.loggerProvider.Log($"[CommandRetrieverMiddleware] Command is unauthorized for {context.RequestUrl.AbsolutePath}");

                Type unauthorizedResponseType = standardResponseProvider.GetResponseType(DefaultStatusCodes.Unauthorized);
                httpResponse = (IHttpResponse)this.serviceProvider.GetService(unauthorizedResponseType);
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
