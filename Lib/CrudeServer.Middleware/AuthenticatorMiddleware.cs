using System;
using System.Security.Principal;
using System.Threading.Tasks;

using CrudeServer.MiddlewareRegistration.Contracts;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class AuthenticatorMiddleware : IMiddleware
    {
        private readonly IAuthenticationProvider _authenticationProvider;

        public AuthenticatorMiddleware(IAuthenticationProvider authenticationProvider)
        {
            this._authenticationProvider = authenticationProvider;
        }

        public async Task Process(RequestContext context, Func<Task> next)
        {
            IPrincipal user = await this._authenticationProvider.GetUser(context);
            if (user != null)
            {
                context.User = user;
            }

            await next();
        }
    }
}
