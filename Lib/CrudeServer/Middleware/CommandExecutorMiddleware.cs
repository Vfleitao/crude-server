using System;
using System.Collections.Generic;
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
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class CommandExecutorMiddleware : IMiddleware
    {
        private readonly ICommandRegistry _commandRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpRequestDataProvider httpRequestDataProvider;
        private readonly ILogger loggerProvider;

        public CommandExecutorMiddleware(
            ICommandRegistry commandRegistry,
            IServiceProvider serviceProvider,
            IHttpRequestDataProvider httpRequestDataProvider,
            ILogger loggerProvider
        )
        {
            this._commandRegistry = commandRegistry;
            this._serviceProvider = serviceProvider;
            this.httpRequestDataProvider = httpRequestDataProvider;
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
                this.loggerProvider.Log($"[0] Command not found for {context.RequestUrl.AbsolutePath}");

                httpResponse = new NotFoundResponse();
            }
            else if (commandRegistration.RequiresAuthentication && !IsUserAuthenticated(commandRegistration, context))
            {
                this.loggerProvider.Log($"[1] Command is unauthorized for {context.RequestUrl.AbsolutePath}");

                httpResponse = new UnauthorizedResponse();
            }
            else
            {
                HttpCommand command = (HttpCommand)this._serviceProvider.GetService(commandRegistration.Command);

                if (command == null)
                {

                    this.loggerProvider.Log($"[2] Command not found for {context.RequestUrl.AbsolutePath}");
                    httpResponse = new NotFoundResponse();
                }
                else
                {
                    this.loggerProvider.Log($"[3] Command found for {context.RequestUrl.AbsolutePath}");

                    HttpRequestData data = await httpRequestDataProvider.GetDataFromRequest(context);
                    UpdateRequestContext(data, context);

                    command.SetContext(context);
                    httpResponse = await command.ExecuteRequest();
                }
            }

            context.Response = httpResponse;

            await next();
        }

        private void UpdateRequestContext(HttpRequestData data, ICommandContext context)
        {
            if (data == null)
            {
                return;
            }

            foreach (KeyValuePair<string, object> item in data.Data)
            {
                context.Items.Add(item.Key, item.Value);
            }

            foreach (HttpFile file in data.Files)
            {
                context.Files.Add(file);
            }
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
