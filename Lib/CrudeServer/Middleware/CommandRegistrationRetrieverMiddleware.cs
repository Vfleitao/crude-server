using System;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware
{
    public class CommandRegistrationRetrieverMiddleware : IMiddleware
    {
        private readonly ICommandRegistry _commandRegistry;

        public CommandRegistrationRetrieverMiddleware(ICommandRegistry commandRegistry)
        {
            this._commandRegistry = commandRegistry;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            HttpMethod httpMethod = context.RequestHttpMethod;

            context.HttpRegistration = _commandRegistry.GetCommand(
                context.RequestUrl.AbsolutePath,
                httpMethod
            );

            await next();
        }
    }
}
