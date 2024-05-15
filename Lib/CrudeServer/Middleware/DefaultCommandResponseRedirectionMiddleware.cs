using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Microsoft.Extensions.Options;

namespace CrudeServer.Middleware
{
    public class DefaultCommandResponseRedirectionMiddleware : IMiddleware
    {
        private readonly IDictionary<int, (string location, int redirectStatusCode)> defaultStatusCodePaths = new Dictionary<int, (string location, int redirectStatusCode)>();
        private readonly IOptions<ServerConfiguration> _serverConfiguraton;
        private readonly ILogger loggerProvider;

        public DefaultCommandResponseRedirectionMiddleware(IOptions<ServerConfiguration> serverConfiguraton, ILogger loggerProvider)
        {
            this._serverConfiguraton = serverConfiguraton;
            this.loggerProvider = loggerProvider;
            if (!string.IsNullOrEmpty(this._serverConfiguraton.Value.AuthenticationPath))
            {
                this.defaultStatusCodePaths.Add(401, (this._serverConfiguraton.Value.AuthenticationPath, 302));
            }
            if (!string.IsNullOrEmpty(this._serverConfiguraton.Value.NotFoundPath) && this._serverConfiguraton.Value.RedirectOnNotFound)
            {
                this.defaultStatusCodePaths.Add(404, (this._serverConfiguraton.Value.NotFoundPath, 302));
            }
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            if (context.Response.StatusCode != 200 &&
                defaultStatusCodePaths.ContainsKey(context.Response.StatusCode) &&
                (!context.IsAjaxRequest ||
                    (this._serverConfiguraton.Value.RedirectOnAjaxCalls && context.IsAjaxRequest)
                )
            )
            {
                loggerProvider.Log($"[DefaultCommandResponseRedirectionMiddleware] Redirecting to {defaultStatusCodePaths[context.Response.StatusCode].location} for status code {context.Response.StatusCode}");

                (string location, int redirectStatusCode) redirectSetup = defaultStatusCodePaths[context.Response.StatusCode];
                context.Response = new RedirectResponse(redirectSetup.location, redirectSetup.redirectStatusCode);
            }

            await next();
        }
    }
}
