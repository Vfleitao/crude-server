using System;
using System.Linq;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.Options;

namespace CrudeServer.Middleware
{
    public class AntiforgeryTokenValidationMiddleware : IMiddleware
    {
        private readonly IOptions<ServerConfiguration> serverConfig;

        public AntiforgeryTokenValidationMiddleware(IOptions<ServerConfiguration> serverConfig)
        {
            this.serverConfig = serverConfig;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            if (context.HttpRegistration == null)
            {
                throw new ArgumentNullException("Http Registration is null. Middleware may be executing too early");
            }

            if(!context.HttpRegistration.RequiresAntiforgeryToken)
            {
                await next();
                return;
            }

            if (
                (context.RequestHttpMethod == Enums.HttpMethod.POST ||
                context.RequestHttpMethod == Enums.HttpMethod.PUT ||
                context.RequestHttpMethod == Enums.HttpMethod.DELETE) &&
                !this.TokenIsValid(context)
            )
            {
                context.Response = new BadRequestResponse();
                return;
            }


            await next();
        }

        private bool TokenIsValid(ICommandContext context)
        {
            if (!context.Items.ContainsKey(this.serverConfig.Value.AntiforgeryTokenInputName))
            {
                return false;
            }

            if (!context.RequestCookies.Any(x => x.Name == this.serverConfig.Value.AntiforgeryTokenCookieName))
            {
                return false;
            }

            string token = context.Items[this.serverConfig.Value.AntiforgeryTokenInputName].ToString();
            string cookieToken = context.RequestCookies.First(x => x.Name == this.serverConfig.Value.AntiforgeryTokenCookieName).Value;

            return string.Equals(token, cookieToken);
        }
    }
}
