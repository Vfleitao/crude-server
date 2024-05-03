using System;
using System.Linq;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware
{
    public class AntiforgeryTokenGenerationMiddleware : IMiddleware
    {
        private readonly IServerConfig serverConfig;

        public AntiforgeryTokenGenerationMiddleware(IServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            if (context.HttpRegistration == null)
            {
                throw new ArgumentNullException("Http Registration is null. Middleware may be executing too early");
            }

            if (context.RequestHttpMethod == HttpMethod.GET || 
                context.RequestHttpMethod == HttpMethod.OPTIONS ||
                context.RequestHttpMethod == HttpMethod.HEAD
            )
            {
                CreateCookie(context);
            }

            await next();
        }

        private void CreateCookie(ICommandContext context)
        {
            if (context.RequestCookies.Any(x => x.Name == this.serverConfig.AntiforgeryTokenCookieName))
            {
                return;
            }

            string token = Guid.NewGuid().ToString();
            context.ResponseCookies.Add(new HttpCookie() { 
                Secure = true,
                HttpOnly = true,
                Name = this.serverConfig.AntiforgeryTokenCookieName,
                Value = token,
                Path = "/",
            });
        }
    }
}
