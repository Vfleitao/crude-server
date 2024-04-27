using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class DefaultCommandResponseRedirectionMiddleware : IMiddleware
    {
        private readonly IDictionary<int, (string location, int redirectStatusCode)> defaultStatusCodePaths = new Dictionary<int, (string location, int redirectStatusCode)>();
        private readonly IServerConfig _serverConfiguraton;
        private readonly ILogger loggerProvider;

        public DefaultCommandResponseRedirectionMiddleware(IServerConfig serverConfiguraton, ILogger loggerProvider)
        {
            this._serverConfiguraton = serverConfiguraton;
            this.loggerProvider = loggerProvider;
            if (!string.IsNullOrEmpty(this._serverConfiguraton.AuthenticationPath))
            {
                this.defaultStatusCodePaths.Add(401, (this._serverConfiguraton.AuthenticationPath, 302));
            }
            if (!string.IsNullOrEmpty(this._serverConfiguraton.NotFoundPath))
            {
                this.defaultStatusCodePaths.Add(404, (this._serverConfiguraton.NotFoundPath, 302));
            }
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            if (context.Response.StatusCode != 200 &&
                defaultStatusCodePaths.ContainsKey(context.Response.StatusCode) &&
                (!context.IsAjaxRequest ||
                    (this._serverConfiguraton.RedirectOnAjaxCalls && context.IsAjaxRequest)
                )
            )
            {
                loggerProvider.Log($"[6] Redirecting to {defaultStatusCodePaths[context.Response.StatusCode].location} for status code {context.Response.StatusCode}");

                (string location, int redirectStatusCode) redirectSetup = defaultStatusCodePaths[context.Response.StatusCode];
                context.Response = new RedirectResponse(redirectSetup.location, redirectSetup.redirectStatusCode);
            }

            await next();
        }
    }
}
