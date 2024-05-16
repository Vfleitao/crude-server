using System;
using System.Linq;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.Options;

namespace CrudeServer.Middleware
{
    public class AntiforgeryTokenGenerationMiddleware : IMiddleware
    {
        private readonly IOptions<ServerConfiguration> serverConfig;

        public AntiforgeryTokenGenerationMiddleware(IOptions<ServerConfiguration> serverConfig)
        {
            this.serverConfig = serverConfig;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
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
            if (context.RequestCookies.Any(x => x.Name == this.serverConfig.Value.AntiforgeryTokenCookieName))
            {
                return;
            }

            string token = Guid.NewGuid().ToString();
            context.ResponseCookies.Add(new HttpCookie()
            {
                Secure = true,
                HttpOnly = true,
                Name = this.serverConfig.Value.AntiforgeryTokenCookieName,
                Value = token,
                Path = "/",
            });
        }
    }
}
