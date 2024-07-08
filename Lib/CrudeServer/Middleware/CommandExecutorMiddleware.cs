using System;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Middleware
{
    public class CommandExecutorMiddleware : IMiddleware
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger loggerProvider;
        private readonly IStandardResponseRegistry standardResponseRegistry;

        public CommandExecutorMiddleware(
            IServiceProvider serviceProvider,
            ILogger loggerProvider,
            IStandardResponseRegistry standardResponseRegistry
        )
        {
            this._serviceProvider = serviceProvider;
            this.loggerProvider = loggerProvider;
            this.standardResponseRegistry = standardResponseRegistry;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            try
            {
                // Already has a response, no need to execute command
                if (context.Response != null)
                {
                    await next();
                    return;
                }

                // No registration found for the request
                if (context.HttpRegistration == null)
                {
                    this.loggerProvider.Log($"[CommandExecutorMiddleware] HttpRegistration not found for {context.RequestUrl.AbsolutePath}");
                    context.Response = new NotFoundResponse();
                    await next();

                    return;
                }

                IHttpResponse httpResponse = null;

                HttpCommandRegistration httpRegistration = context.HttpRegistration;

                HttpCommand command;

                if (httpRegistration.CommandFunction != null)
                {
                    HttpFunctionCommand functionHttpCommand = this._serviceProvider.GetService<HttpFunctionCommand>();
                    functionHttpCommand.DelegateFunction = httpRegistration.CommandFunction;

                    command = functionHttpCommand;
                }
                else
                {
                    command = (HttpCommand)this._serviceProvider.GetService(httpRegistration.Command);
                }

                if (command == null)
                {
                    this.loggerProvider.Log($"[CommandExecutorMiddleware] Command not found in IOC for {context.HttpRegistration.Command}");
                    httpResponse = new NotFoundResponse();
                }
                else
                {
                    this.loggerProvider.Log($"[CommandExecutorMiddleware] Command found for {context.RequestUrl.AbsolutePath}");
                    httpResponse = await command.ExecuteRequest();
                }

                context.Response = httpResponse;

            }
            catch (Exception ex)
            {
                this.loggerProvider.Error("[CommandExecutorMiddleware] Error Executing command", ex);

                Type responseType = this.standardResponseRegistry.GetResponseType(DefaultStatusCodes.InternalError);
                context.Response = this._serviceProvider.GetService(responseType) as IHttpResponse;
            }

            await next();
        }
    }
}
