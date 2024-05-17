using System;
using System.Security.Principal;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Authentication;
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

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            IPrincipal user = await this._authenticationProvider.GetUserFromHeaders(context);
            if (user == null)
            {
                user = await this._authenticationProvider.GetUserFromCookies(context);
            }

            if (user != null)
            {
                context.User = new UserWrapper(user);
            }

            await next();
        }
    }
}
