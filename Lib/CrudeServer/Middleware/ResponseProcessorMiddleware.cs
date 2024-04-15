using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Middleware
{
    public class ResponseProcessorMiddleware : IMiddleware
    {
        private readonly IDictionary<int, (string location, int redirectStatusCode)> defaultStatusCodePaths = new Dictionary<int, (string location, int redirectStatusCode)>();
        private readonly IServerConfig _serverConfiguraton;

        public ResponseProcessorMiddleware(IServerConfig serverConfiguraton)
        {
            this._serverConfiguraton = serverConfiguraton;

            if (!string.IsNullOrEmpty(this._serverConfiguraton.AuthenticationPath))
            {
                this.defaultStatusCodePaths.Add(401, (this._serverConfiguraton.AuthenticationPath, 302));
            }
            if (!string.IsNullOrEmpty(this._serverConfiguraton.NotFoundPath))
            {
                this.defaultStatusCodePaths.Add(404, (this._serverConfiguraton.NotFoundPath, 302));
            }
        }

        public async Task Process(IRequestContext context, Func<Task> next)
        {
            IHttpResponse httpResponse = context.Response;

            if (context.Response.StatusCode != 200 &&
                defaultStatusCodePaths.ContainsKey(context.Response.StatusCode) &&
                (!context.IsAjaxRequest ||
                    (this._serverConfiguraton.RedirectOnAjaxCalls && context.IsAjaxRequest)
                )
            )
            {
                (string location, int redirectStatusCode) redirectSetup = defaultStatusCodePaths[context.Response.StatusCode];
                httpResponse = new RedirectResponse(redirectSetup.location, redirectSetup.redirectStatusCode);
            }

            context.HttpListenerResponse.StatusCode = httpResponse.StatusCode;
            context.HttpListenerResponse.ContentType = httpResponse.ContentType;

            if (httpResponse is RedirectResponse)
            {
                context.HttpListenerResponse.RedirectLocation = Encoding.UTF8.GetString(httpResponse.ResponseData);
            }
            else if (httpResponse.ResponseData != null && httpResponse.ResponseData.Length > 0)
            {
                await context.HttpListenerResponse.OutputStream.WriteAsync(httpResponse.ResponseData, 0, httpResponse.ResponseData.Length);
            }

            await next();
        }
    }
}
