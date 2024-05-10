using System;
using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class CommandExecutorMiddleware : IMiddleware
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger loggerProvider;

        public CommandExecutorMiddleware(
            IServiceProvider serviceProvider,
            ILogger loggerProvider
        )
        {
            this._serviceProvider = serviceProvider;
            this.loggerProvider = loggerProvider;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
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

            HttpCommand command = (HttpCommand)this._serviceProvider.GetService(context.HttpRegistration.Command);
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

            await next();
        }
    }
}
